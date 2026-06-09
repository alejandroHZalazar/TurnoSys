# -*- coding: utf-8 -*-
"""Manual de Usuario de TurnoSys en PDF (tratamiento de usted + capturas de pantalla)."""

import os
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import cm
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER, TA_JUSTIFY
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, Image as RLImage,
    PageBreak, ListFlowable, ListItem, HRFlowable, KeepTogether
)

INDIGO      = colors.HexColor("#4F46E5")
INDIGO_DARK = colors.HexColor("#3730A3")
INDIGO_SOFT = colors.HexColor("#EEF2FF")
GRIS_TXT    = colors.HexColor("#374151")
GRIS_SUAVE  = colors.HexColor("#6B7280")
GRIS_LINEA  = colors.HexColor("#D1D5DB")
VERDE       = colors.HexColor("#059669")
VERDE_SOFT  = colors.HexColor("#ECFDF5")
AMBAR       = colors.HexColor("#B45309")
AMBAR_SOFT  = colors.HexColor("#FFFBEB")
BLANCO      = colors.white
BASE = os.path.dirname(os.path.abspath(__file__))
CAP  = os.path.join(BASE, "capturas")
ARCHIVO = os.path.join(BASE, "Manual_Usuario_TurnoSys.pdf")

styles = getSampleStyleSheet()
def S(name, **kw): styles.add(ParagraphStyle(name, **kw))
S("Portada",    fontName="Helvetica-Bold", fontSize=34, leading=40, textColor=INDIGO_DARK, alignment=TA_CENTER)
S("PortadaSub", fontName="Helvetica",      fontSize=15, leading=22, textColor=GRIS_SUAVE,  alignment=TA_CENTER)
S("H1",         fontName="Helvetica-Bold", fontSize=18, leading=24, textColor=INDIGO_DARK, spaceBefore=6, spaceAfter=10)
S("H2",         fontName="Helvetica-Bold", fontSize=13, leading=18, textColor=INDIGO,      spaceBefore=12, spaceAfter=5)
S("Body",       fontName="Helvetica",      fontSize=10.5, leading=16, textColor=GRIS_TXT,  alignment=TA_JUSTIFY, spaceAfter=6)
S("TSBullet",   fontName="Helvetica",      fontSize=10.5, leading=15, textColor=GRIS_TXT)
S("Nota",       fontName="Helvetica",      fontSize=10, leading=15, textColor=GRIS_TXT)
S("NotaTit",    fontName="Helvetica-Bold", fontSize=10, leading=14, textColor=INDIGO_DARK)
S("Celda",      fontName="Helvetica",      fontSize=9.5, leading=13, textColor=GRIS_TXT)
S("CeldaHead",  fontName="Helvetica-Bold", fontSize=9.5, leading=13, textColor=BLANCO)
S("Paso",       fontName="Helvetica",      fontSize=10.5, leading=15, textColor=GRIS_TXT)
S("TOCTitle",   fontName="Helvetica-Bold", fontSize=18, leading=24, textColor=INDIGO_DARK, spaceAfter=14)
S("TOCNum",     fontName="Helvetica-Bold", fontSize=11, leading=18, textColor=INDIGO)
S("TOCItem",    fontName="Helvetica",      fontSize=11, leading=18, textColor=GRIS_TXT)
S("Caption",    fontName="Helvetica-Oblique", fontSize=9, leading=12, textColor=GRIS_SUAVE, alignment=TA_CENTER)

def header_footer(canvas, doc):
    canvas.saveState(); w, h = A4
    canvas.setFillColor(INDIGO); canvas.rect(0, h - 1.15*cm, w, 1.15*cm, stroke=0, fill=1)
    canvas.setFillColor(BLANCO)
    canvas.setFont("Helvetica-Bold", 10); canvas.drawString(2*cm, h - 0.78*cm, "TurnoSys")
    canvas.setFont("Helvetica", 8.5); canvas.drawRightString(w - 2*cm, h - 0.78*cm, "Manual de Usuario")
    canvas.setStrokeColor(GRIS_LINEA); canvas.setLineWidth(0.5); canvas.line(2*cm, 1.3*cm, w - 2*cm, 1.3*cm)
    canvas.setFillColor(GRIS_SUAVE); canvas.setFont("Helvetica", 8)
    canvas.drawString(2*cm, 0.9*cm, "Sistema de gestion de turnos")
    canvas.drawRightString(w - 2*cm, 0.9*cm, f"Pagina {doc.page - 1}")
    canvas.restoreState()

def portada(canvas, doc):
    canvas.saveState(); w, h = A4
    canvas.setFillColor(INDIGO_DARK); canvas.rect(0, h - 6*cm, w, 6*cm, stroke=0, fill=1)
    canvas.setFillColor(INDIGO); canvas.rect(0, h - 6.35*cm, w, 0.35*cm, stroke=0, fill=1)
    canvas.setFillColor(BLANCO); canvas.setFont("Helvetica-Bold", 32)
    canvas.drawCentredString(w/2, h - 3.6*cm, "TurnoSys")
    canvas.setFillColor(colors.HexColor("#C7D2FE")); canvas.setFont("Helvetica", 13)
    canvas.drawCentredString(w/2, h - 4.6*cm, "Gestion de turnos medicos y esteticos")
    canvas.restoreState()

def p(txt, st="Body"): return Paragraph(txt, styles[st])
def vsp(x=6): return Spacer(1, x)
def vinheta(items):
    its = [ListItem(Paragraph(t, styles["TSBullet"]), leftIndent=10) for t in items]
    return ListFlowable(its, bulletType="bullet", bulletColor=INDIGO, leftIndent=14,
                        bulletFontSize=7, spaceBefore=2, spaceAfter=6)
def pasos(items):
    its = [ListItem(Paragraph(t, styles["Paso"]), leftIndent=12) for t in items]
    return ListFlowable(its, bulletType="1", bulletColor=INDIGO, bulletFontName="Helvetica-Bold",
                        leftIndent=18, bulletFontSize=10, spaceBefore=2, spaceAfter=6)
def callout(titulo, texto, tipo="info"):
    if tipo == "ok":   bg, brd, tc = VERDE_SOFT, VERDE, VERDE
    elif tipo == "warn": bg, brd, tc = AMBAR_SOFT, AMBAR, AMBAR
    else:              bg, brd, tc = INDIGO_SOFT, INDIGO, INDIGO_DARK
    inner = [Paragraph(titulo, ParagraphStyle("ct", parent=styles["NotaTit"], textColor=tc)),
             Paragraph(texto, styles["Nota"])]
    t = Table([[inner]], colWidths=[16.0*cm])
    t.setStyle(TableStyle([("BACKGROUND",(0,0),(-1,-1),bg),("LINEBEFORE",(0,0),(0,-1),3,brd),
        ("LEFTPADDING",(0,0),(-1,-1),12),("RIGHTPADDING",(0,0),(-1,-1),12),
        ("TOPPADDING",(0,0),(-1,-1),8),("BOTTOMPADDING",(0,0),(-1,-1),8),("BOX",(0,0),(-1,-1),0.5,brd)]))
    return KeepTogether([t, vsp(8)])
def tabla(headers, filas, anchos):
    data = [[Paragraph(h, styles["CeldaHead"]) for h in headers]]
    for f in filas: data.append([Paragraph(str(c), styles["Celda"]) for c in f])
    t = Table(data, colWidths=anchos, repeatRows=1)
    est = [("BACKGROUND",(0,0),(-1,0),INDIGO),("LEFTPADDING",(0,0),(-1,-1),7),("RIGHTPADDING",(0,0),(-1,-1),7),
        ("TOPPADDING",(0,0),(-1,-1),6),("BOTTOMPADDING",(0,0),(-1,-1),6),("VALIGN",(0,0),(-1,-1),"TOP"),
        ("LINEBELOW",(0,0),(-1,-1),0.5,GRIS_LINEA),("BOX",(0,0),(-1,-1),0.5,GRIS_LINEA)]
    for i in range(1, len(data)):
        if i % 2 == 0: est.append(("BACKGROUND",(0,i),(-1,i),colors.HexColor("#F9FAFB")))
    t.setStyle(TableStyle(est)); return KeepTogether([t, vsp(8)])

def captura(nombre, caption, ancho=15.4*cm):
    """Inserta una captura centrada, con marco, sin deformar ni salir de margen."""
    path = os.path.join(CAP, nombre + ".png")
    img = RLImage(path)
    ratio = img.imageHeight / float(img.imageWidth)
    img.drawWidth = ancho
    img.drawHeight = ancho * ratio
    marco = Table([[img]], colWidths=[ancho])
    marco.hAlign = "CENTER"
    marco.setStyle(TableStyle([("BOX",(0,0),(-1,-1),0.75,GRIS_LINEA),
        ("LEFTPADDING",(0,0),(-1,-1),0),("RIGHTPADDING",(0,0),(-1,-1),0),
        ("TOPPADDING",(0,0),(-1,-1),0),("BOTTOMPADDING",(0,0),(-1,-1),0)]))
    cap = Paragraph(caption, styles["Caption"]); cap.hAlign = "CENTER"
    return KeepTogether([vsp(4), marco, vsp(3), cap, vsp(10)])

# ===================== CONTENIDO =====================
story = []

# Portada
story += [Spacer(1, 7.2*cm), p("Manual de Usuario", "Portada"), vsp(10),
          p("Guia completa para el uso diario del sistema", "PortadaSub"),
          Spacer(1, 8.5*cm), p("Version 1.0", "PortadaSub"), PageBreak()]

# Indice
story.append(Paragraph("Contenido", styles["TOCTitle"]))
indice = [("1","Introduccion"),("2","Acceso al sistema"),("3","La Agenda"),("4","Pacientes"),
          ("5","Profesionales"),("6","Practicas"),("7","Panel de control (Dashboard)"),
          ("8","Datos de la empresa"),("9","Configuracion"),("10","Recordatorios automaticos"),
          ("11","Recomendaciones finales")]
filas_idx = [[Paragraph(n, styles["TOCNum"]), Paragraph(t, styles["TOCItem"])] for n,t in indice]
t_idx = Table(filas_idx, colWidths=[1.2*cm, 14.8*cm])
t_idx.setStyle(TableStyle([("LINEBELOW",(0,0),(-1,-2),0.4,GRIS_LINEA),
    ("TOPPADDING",(0,0),(-1,-1),7),("BOTTOMPADDING",(0,0),(-1,-1),7),("VALIGN",(0,0),(-1,-1),"MIDDLE")]))
story += [t_idx, PageBreak()]

# 1. Introduccion
story.append(Paragraph("1. Introduccion", styles["H1"]))
story.append(p("TurnoSys es un sistema para administrar los turnos de su consultorio, clinica o centro de estetica. "
               "Desde una sola pantalla usted puede organizar la agenda de sus profesionales, registrar pacientes, "
               "definir las practicas que ofrece y enviar recordatorios automaticos por correo."))
story.append(Paragraph("Que va a poder hacer", styles["H2"]))
story.append(vinheta([
    "Ver y cargar turnos en una agenda visual (dia, semana, mes o lista).",
    "Registrar pacientes y profesionales con todos sus datos.",
    "Definir practicas con precio, duracion y color.",
    "Recibir recordatorios automaticos de controles a futuro.",
    "Consultar estadisticas del negocio en el panel de control.",
]))
story.append(callout("Antes de empezar",
    "Para usar el sistema usted necesita un usuario y una contrasena que le entrega el administrador. "
    "Si no los tiene, solicitelos antes de continuar."))

# 2. Acceso
story.append(Paragraph("2. Acceso al sistema", styles["H1"]))
story.append(p("Para ingresar a TurnoSys abra la direccion del sistema en su navegador (Chrome, Edge o Firefox) "
               "y complete la pantalla de inicio de sesion."))
story.append(captura("login", "Pantalla de inicio de sesion."))
story.append(pasos([
    "Ingrese su <b>correo electronico</b> y su <b>contrasena</b>.",
    "Presione el boton <b>Ingresar</b>.",
    "Si los datos son correctos, accedera directamente a la Agenda.",
]))
story.append(callout("Si no puede ingresar",
    "Verifique que el correo y la contrasena esten bien escritos. Tras varios intentos fallidos, "
    "la cuenta se bloquea unos minutos por seguridad: espere y vuelva a intentar.", "warn"))

# 3. Agenda
story.append(Paragraph("3. La Agenda", styles["H1"]))
story.append(p("La Agenda es la pantalla principal. Muestra los turnos en una grilla de horarios y permite crear, "
               "ver, reprogramar, atender o cancelar cada turno."))
story.append(captura("agenda", "Vista de la Agenda con el menu lateral y los turnos del periodo."))
story.append(Paragraph("Cambiar la vista", styles["H2"]))
story.append(p("Con los botones de la parte superior derecha usted puede alternar entre las vistas <b>Dia</b>, "
               "<b>Semana</b>, <b>Mes</b> y <b>Lista</b>. Use las flechas para moverse entre fechas."))
story.append(Paragraph("Crear un turno", styles["H2"]))
story.append(p("Haga clic en el horario deseado dentro de la grilla, o use el boton <b>Nuevo turno</b>. "
               "Se abrira la siguiente ventana:"))
story.append(captura("nuevo_turno", "Ventana para registrar un nuevo turno."))
story.append(pasos([
    "Busque y seleccione el <b>paciente</b> escribiendo su nombre, apellido o DNI. "
    "Si es nuevo, use <b>+ Nuevo</b> para darlo de alta al instante.",
    "Elija el <b>profesional</b> y la <b>practica</b> (la duracion se calcula sola).",
    "Confirme la <b>fecha y hora</b>. Opcionalmente cargue observaciones.",
    "Presione <b>Guardar turno</b>.",
]))
story.append(callout("Control a futuro",
    "Al crear el turno usted puede indicar una fecha de <b>proximo control sugerido</b>. Esto NO reserva otro "
    "turno: solo genera un recordatorio que el sistema enviara por correo al profesional, dias antes, para que "
    "contacte al paciente."))
story.append(Paragraph("Acciones sobre un turno", styles["H2"]))
story.append(p("Al hacer clic sobre un turno existente se abre su detalle, donde usted puede:"))
story.append(tabla(["Accion", "Que hace"],
    [["Atender", "Marca el turno como realizado (estado Atendido)."],
     ["Reagendar", "Cambia el turno a una nueva fecha y hora."],
     ["Cancelar", "Anula el turno. El horario vuelve a quedar libre para otro paciente."]],
    [3.5*cm, 12.5*cm]))
story.append(callout("Estados de un turno",
    "<b>Reservado</b> (turno activo) &nbsp; <b>Atendido</b> (ya se realizo) &nbsp; "
    "<b>Cancelado</b> (anulado, libera el horario). Cada estado tiene su color en la agenda."))

# 4. Pacientes
story.append(Paragraph("4. Pacientes", styles["H1"]))
story.append(p("En la seccion <b>Pacientes</b> usted administra la ficha de cada persona que atiende."))
story.append(captura("pacientes", "Listado de pacientes con buscador y acciones."))
story.append(Paragraph("Buscar y filtrar", styles["H2"]))
story.append(p("Use el buscador para encontrar pacientes por nombre, apellido, DNI o telefono. "
               "Con el filtro <b>Solo activos</b> oculta los dados de baja."))
story.append(Paragraph("Alta y edicion", styles["H2"]))
story.append(pasos([
    "Presione <b>Nuevo paciente</b> (o el lapiz para editar uno existente).",
    "Complete los datos. Solo <b>Apellido</b> y <b>Nombre</b> son obligatorios.",
    "Puede cargar DNI, fecha de nacimiento, contacto, obra social, contacto de emergencia y notas clinicas.",
    "Presione <b>Crear paciente</b> o <b>Guardar cambios</b>.",
]))
story.append(callout("Eliminar un paciente",
    "Un paciente con turnos futuros reservados no se puede eliminar. Primero cancele o reprograme esos turnos.", "warn"))

# 5. Profesionales
story.append(Paragraph("5. Profesionales", styles["H1"]))
story.append(p("Aqui usted carga a los profesionales que atienden, junto con sus horarios de trabajo y un color "
               "para identificarlos en la agenda."))
story.append(captura("profesionales", "Profesionales registrados, con sus horarios y color de agenda."))
story.append(pasos([
    "Presione <b>Nuevo profesional</b>.",
    "Complete apellido, nombre, especialidad, matricula y datos de contacto.",
    "Elija un <b>color de agenda</b> de la paleta (o uno personalizado).",
    "Agregue sus <b>horarios laborales</b>: por cada dia, la hora de inicio y fin.",
    "Guarde los cambios.",
]))
story.append(callout("Color de agenda",
    "El color que elija se usa para pintar todos los turnos de ese profesional en la grilla, lo que facilita "
    "distinguir de un vistazo quien atiende cada turno."))

# 6. Practicas
story.append(Paragraph("6. Practicas", styles["H1"]))
story.append(p("Las practicas son los servicios que usted ofrece (por ejemplo: consulta, limpieza facial, control). "
               "Cada una define su precio, duracion y color."))
story.append(captura("practicas", "Practicas agrupadas por categoria, con precio y duracion."))
story.append(tabla(["Campo", "Para que sirve"],
    [["Nombre", "Como se llama la practica."],
     ["Precio", "Valor que se usa para estimar ingresos en las estadisticas."],
     ["Duracion", "Cuanto dura; define automaticamente el horario de fin del turno."],
     ["Color", "Identificacion visual."],
     ["Requiere observaciones", "Marca la practica para recordar cargar notas."],
     ["Recordatorio control (dias)", "Sugerencia de cada cuantos dias conviene un control."]],
    [5.2*cm, 10.8*cm]))
story.append(callout("Categorias",
    "Usted puede agrupar las practicas por categoria (por ejemplo Clinica o Estetica) para mantenerlas "
    "ordenadas. La categoria es opcional."))

# 7. Dashboard
story.append(Paragraph("7. Panel de control (Dashboard)", styles["H1"]))
story.append(p("El Dashboard resume la actividad del negocio con indicadores y graficos. Usted puede filtrar por "
               "<b>ultimos 7, 30 o 90 dias</b>."))
story.append(captura("dashboard", "Panel de control con indicadores y graficos del negocio."))
story.append(Paragraph("Que va a ver", styles["H2"]))
story.append(vinheta([
    "Tarjetas con totales: turnos, atendidos, cancelados, ocupacion, ingresos estimados y pacientes nuevos.",
    "Grafico de turnos por dia y distribucion por estado.",
    "Ranking de profesionales y de pacientes mas frecuentes.",
    "Mapa de calor de ocupacion por dia y hora (para detectar los horarios mas demandados).",
]))

# 8. Empresa
story.append(Paragraph("8. Datos de la empresa", styles["H1"]))
story.append(p("En <b>Empresa</b> usted carga los datos de su negocio: razon social, CUIT, direccion, telefono, "
               "redes sociales, horario de atencion y el <b>logotipo</b>."))
story.append(captura("empresa", "Formulario con los datos de la empresa y el logotipo."))
story.append(pasos([
    "Complete o actualice los campos.",
    "Para el logo, presione <b>Subir logotipo</b> y elija una imagen (PNG, JPG o similar, hasta 2 MB).",
    "Presione <b>Guardar cambios</b>.",
]))

# 9. Configuracion
story.append(Paragraph("9. Configuracion", styles["H1"]))
story.append(p("Desde <b>Configuracion</b> usted define las reglas generales del sistema, organizadas en tres "
               "secciones."))
story.append(captura("configuracion", "Pantalla de configuracion: turnos, recordatorios y correo."))
story.append(tabla(["Seccion", "Que configura"],
    [["Turnos", "Duracion por defecto, horario de apertura y cierre, dias de atencion."],
     ["Recordatorios", "Dias de anticipacion y hora a la que se envian los avisos."],
     ["Email", "Proveedor de correo y credenciales para que salgan los recordatorios."]],
    [3.8*cm, 12.2*cm]))
story.append(callout("Horarios de la agenda",
    "El horario de apertura y cierre que usted defina en <b>Turnos</b> determina la franja que muestra la agenda. "
    "Si lo cambia, la grilla se ajusta automaticamente."))
story.append(callout("Probar el correo",
    "En la seccion Email tiene un boton <b>Enviar prueba</b>. Uselo despues de cargar las credenciales para "
    "confirmar que los correos salen correctamente antes de depender de ellos.", "ok"))

# 10. Recordatorios
story.append(Paragraph("10. Recordatorios automaticos", styles["H1"]))
story.append(p("El sistema envia por correo, de forma automatica, los recordatorios de controles a futuro que "
               "usted cargo al crear un turno."))
story.append(Paragraph("Como funciona", styles["H2"]))
story.append(pasos([
    "Al crear un turno usted indica una fecha de <b>proximo control sugerido</b>.",
    "El sistema calcula cuando avisar: esa fecha menos los <b>dias de anticipacion</b> configurados.",
    "Ese dia, a la <b>hora de envio</b> configurada, se envia el correo al profesional.",
    "El correo incluye un boton de <b>WhatsApp</b> para contactar al paciente con un solo clic.",
]))
story.append(callout("Importante",
    "Los recordatorios solo se envian si el sistema esta en funcionamiento a la hora programada. "
    "En un servidor en la nube esto es automatico y permanente.", "warn"))

# 11. Recomendaciones
story.append(Paragraph("11. Recomendaciones finales", styles["H1"]))
story.append(vinheta([
    "Cargue primero <b>profesionales</b> y <b>practicas</b>: son la base para poder reservar turnos.",
    "Mantenga los datos de los pacientes actualizados, sobre todo el <b>telefono</b> (se usa para WhatsApp).",
    "Revise periodicamente el <b>Dashboard</b> para entender la marcha del negocio.",
    "Cambie su contrasena inicial por una personal y segura.",
]))
story.append(vsp(10))
story.append(HRFlowable(width="100%", color=GRIS_LINEA, thickness=0.6))
story.append(vsp(6))
story.append(Paragraph("TurnoSys - Manual de Usuario - Version 1.0",
    ParagraphStyle("fin", fontName="Helvetica", fontSize=9, textColor=GRIS_SUAVE, alignment=TA_CENTER)))

doc = SimpleDocTemplate(ARCHIVO, pagesize=A4, leftMargin=2*cm, rightMargin=2*cm,
                        topMargin=1.7*cm, bottomMargin=1.7*cm,
                        title="Manual de Usuario - TurnoSys", author="TurnoSys")
doc.build(story, onFirstPage=portada, onLaterPages=header_footer)
print("OK:", ARCHIVO)
