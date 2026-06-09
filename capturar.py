# -*- coding: utf-8 -*-
"""Toma capturas de pantalla de TurnoSys con Playwright (viewport fijo, alta DPI)."""
import os, time
from playwright.sync_api import sync_playwright

BASE = "http://localhost:5173"
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "capturas")
os.makedirs(OUT, exist_ok=True)

def run():
    with sync_playwright() as pw:
        browser = pw.chromium.launch(headless=True)
        ctx = browser.new_context(viewport={"width": 1280, "height": 820}, device_scale_factor=2)
        page = ctx.new_page()

        def cap(nombre, espera=1.2):
            time.sleep(espera)
            page.screenshot(path=os.path.join(OUT, nombre + ".png"))
            print("captura:", nombre)

        # 1. Login
        page.goto(BASE + "/login", wait_until="networkidle")
        cap("login", 1.0)

        # Login
        page.fill("input[type=email]", "admin@turnosys.com")
        page.fill("input[type=password]", "Admin1234!")
        page.click("button[type=submit]")
        page.wait_for_url("**/agenda", timeout=15000)
        page.wait_for_load_state("networkidle")

        # 2. Agenda
        page.goto(BASE + "/agenda", wait_until="networkidle")
        cap("agenda", 2.0)

        # 3. Modal nuevo turno
        try:
            page.get_by_role("button", name="Nuevo turno").click()
            cap("nuevo_turno", 1.5)
            page.keyboard.press("Escape")
            time.sleep(0.5)
        except Exception as e:
            print("modal turno error:", e)

        # 4. Pacientes
        page.goto(BASE + "/pacientes", wait_until="networkidle")
        cap("pacientes", 1.8)

        # 5. Profesionales
        page.goto(BASE + "/profesionales", wait_until="networkidle")
        cap("profesionales", 1.8)

        # 6. Practicas
        page.goto(BASE + "/practicas", wait_until="networkidle")
        cap("practicas", 1.8)

        # 7. Dashboard
        page.goto(BASE + "/dashboard", wait_until="networkidle")
        cap("dashboard", 2.5)

        # 8. Empresa
        page.goto(BASE + "/empresa", wait_until="networkidle")
        cap("empresa", 1.8)

        # 9. Configuracion
        page.goto(BASE + "/configuracion", wait_until="networkidle")
        cap("configuracion", 1.8)

        browser.close()
        print("Listo. Capturas en:", OUT)

run()
