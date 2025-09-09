from fastapi import FastAPI, Query, HTTPException
from pydantic import BaseModel, Field
from typing import List, Optional, Dict, Any
from datetime import datetime, timezone
import pandas as pd
import numpy as np
import requests
import pvlib

app = FastAPI(title="PV Forecast API (Solcast + pvlib)", version="1.0.0")

# Настройки по умолчанию (обновлены вашими данными)
LAT = 47.0626
LON = 28.8679
TZ = "Europe/Chisinau"  # Установил таймзону для Молдовы, можете поменять
CAPACITY_KW = 3.9       # DC мощность вашей системы
TILT = 30.0
AZIMUTH = 180.0
GAMMA_PDC = -0.004  # 1/°C
ALBEDO = 0.2
API_KEY = "rVsZcjPon_LO1u1Pvkf4k71VRP3QvOI7"  # Убедитесь, что здесь ваш ключ
RESOURCE_ID = "d0d0-6c65-3dea-b637" # Ваш ID объекта из Solcast


class ForecastPoint(BaseModel):
    timestamp: datetime = Field(description="Момент времени в ISO-8601")
    pac_w: float = Field(description="AC мощность, Вт")
    e_wh: float = Field(description="Энергия за шаг, Вт*ч")
    ghi: float = Field(description="GHI, W/m²")
    dni: float = Field(description="DNI, W/m²")
    dhi: float = Field(description="DHI, W/m²")


class ForecastResponse(BaseModel):
    location: dict
    system: dict
    step_hours: float
    points: List[ForecastPoint]
    total_energy_kwh: float
    debug: Optional[Dict[str, Any]] = None


@app.get("/predict", response_model=ForecastResponse)
def predict(
        hours: int = Query(24, ge=1, le=240, description="Количество часов для прогноза"),
        lat: float = Query(LAT),
        lon: float = Query(LON),
        tz: str = Query(TZ),
        capacity_kw: float = Query(CAPACITY_KW, gt=0),
        tilt: float = Query(TILT),
        azimuth: float = Query(AZIMUTH),
        gamma_pdc: float = Query(GAMMA_PDC),
        albedo: float = Query(ALBEDO),
        debug: bool = Query(False)
):
    # --- Получаем прогноз Solcast для зарегистрированного объекта ---
    url = f"https://api.solcast.com.au/rooftop_sites/{RESOURCE_ID}/forecasts"
    headers = {"Authorization": f"Bearer {API_KEY}"}
    params = {
        "format": "json",
        "hours": hours
    }

    try:
        r = requests.get(url, headers=headers, params=params, timeout=20)
        r.raise_for_status()
        data = r.json()
    except requests.HTTPError as ex:
        raise HTTPException(status_code=r.status_code, detail=f"Solcast API error: {ex}")
    except Exception as ex:
        raise HTTPException(status_code=502, detail=f"Solcast request failed: {ex}")

    forecasts = data.get("forecasts", [])
    if not forecasts:
        raise HTTPException(status_code=404, detail="No forecast data from Solcast")

    # --- Создаем DataFrame с радиацией ---
    points = []
    step_h = 0.5  # Прогноз Solcast обычно с шагом 30 минут

    for f in forecasts:
        pac_kw = f.get("pv_estimate", 0.0)  # Получаем готовый прогноз в кВт
        pac_w = pac_kw * 1000.0  # Конвертируем в Вт

        points.append(ForecastPoint(
            timestamp=pd.to_datetime(f["period_end"]).to_pydatetime(),
            pac_w=float(pac_w),
            e_wh=float(pac_w * step_h),  # Энергия за шаг (Вт*ч)
            ghi=float(f.get("ghi", 0.0)),
            dni=float(f.get("dhi", 0.0)), # Опечатка в вашем коде, должно быть dhi
            dhi=float(f.get("dhi", 0.0))
        ))
    
    total_energy_kwh = sum(p.e_wh for p in points) / 1000.0
    

    
    location_meta = {"lat": lat, "lon": lon, "tz": tz}
    system_meta = {"capacity_kw": capacity_kw, "tilt_deg": tilt, "azimuth_deg": azimuth, "gamma_pdc_per_c": gamma_pdc, "albedo": albedo}

    return ForecastResponse(
        location=location_meta,
        system=system_meta,
        step_hours=step_h,
        points=points,
        total_energy_kwh=total_energy_kwh,
        debug={"total_points": len(points)} if debug else None
    )


@app.get("/health")
def health():
    return {"status": "ok"}
