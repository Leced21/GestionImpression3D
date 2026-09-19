from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import A3, landscape
from reportlab.lib import colors
from reportlab.lib.units import mm
from reportlab.pdfbase.pdfmetrics import stringWidth
from pathlib import Path

OUT = Path('output/pdf/plan_technique_vase_lisse.pdf')
OUT.parent.mkdir(parents=True, exist_ok=True)
W, H = landscape(A3)
c = canvas.Canvas(str(OUT), pagesize=(W, H))

navy = colors.HexColor('#172638'); blue = colors.HexColor('#236C9B')
grey = colors.HexColor('#65727E'); light = colors.HexColor('#E8EDF1')
scale = 0.35  # approximately 1:2.86, mm drawing per mm part

def line(x1,y1,x2,y2, stroke=navy, width=.6, dash=None):
    c.setStrokeColor(stroke); c.setLineWidth(width)
    c.setDash(dash or [])
    c.line(x1,y1,x2,y2); c.setDash([])

def text(x,y,s,size=8,color=navy, align='left'):
    c.setFillColor(color); c.setFont('Helvetica',size)
    if align=='center': x -= stringWidth(s,'Helvetica',size)/2
    c.drawString(x,y,s)

def dim_h(x, y, x1, x2, label):
    line(x1,y,x2,y,blue,.55); line(x1,y-3,x1,y+3,blue,.55); line(x2,y-3,x2,y+3,blue,.55)
    line(x1,y,x1+4,y+2,blue,.55); line(x1,y,x1+4,y-2,blue,.55)
    line(x2,y,x2-4,y+2,blue,.55); line(x2,y,x2-4,y-2,blue,.55)
    text((x1+x2)/2,y+4,label,8,blue,'center')

def dim_v(x, y1, y2, label):
    line(x,y1,x,y2,blue,.55); line(x-3,y1,x+3,y1,blue,.55); line(x-3,y2,x+3,y2,blue,.55)
    line(x,y1,x+2,y1+4,blue,.55); line(x,y1,x-2,y1+4,blue,.55)
    line(x,y2,x+2,y2-4,blue,.55); line(x,y2,x-2,y2-4,blue,.55)
    c.saveState(); c.translate(x-5,(y1+y2)/2); c.rotate(90); text(0,0,label,8,blue,'center'); c.restoreState()

def vase_outline(cx, basey, section=False):
    # outer R/Z stations, scaled from millimetres
    pts=[(72.5,0),(75,20),(78,80),(80,150),(79,220),(75,300),(67,380),(55,440),(45,500)]
    l=[]; r=[]
    for rad,z in pts:
        l.append((cx-rad*scale, basey+z*scale)); r.append((cx+rad*scale,basey+z*scale))
    p=c.beginPath(); p.moveTo(*l[0])
    for q in l[1:]: p.lineTo(*q)
    for q in reversed(r): p.lineTo(*q)
    p.close()
    c.setFillColor(colors.white); c.setStrokeColor(navy); c.setLineWidth(1.0); c.drawPath(p,fill=1,stroke=1)
    if section:
        # interior at 3 mm wall, 5 mm floor. Dashed for section readability.
        inn=[(69.5,5),(72,20),(75,80),(77,150),(76,220),(72,300),(64,380),(52,440),(42,500)]
        c.setStrokeColor(grey); c.setLineWidth(.6); c.setDash(3,2)
        c.line(cx-42*scale,basey+500*scale,cx-42*scale,basey+497*scale)
        prev=(cx-inn[0][0]*scale,basey+inn[0][1]*scale)
        for rad,z in inn[1:]:
            now=(cx-rad*scale,basey+z*scale); c.line(*prev,*now); prev=now
        prev=(cx+inn[0][0]*scale,basey+inn[0][1]*scale)
        for rad,z in inn[1:]:
            now=(cx+rad*scale,basey+z*scale); c.line(*prev,*now); prev=now
        c.setDash([])
    # centre axis
    line(cx,basey-8,cx,basey+500*scale+8,grey,.35,[3,2])

# border/header
c.setFillColor(navy); c.rect(0,H-25*mm,W,25*mm,fill=1,stroke=0)
text(18*mm,H-14*mm,'PLAN TECHNIQUE - VASE DE SOL LISSE',16,colors.white)
text(W-18*mm,H-14*mm,'Révision A | Unités : mm | Échelle : 1:2.86',9,colors.white,'right')

# elevation
basey=50*mm; cx=95*mm
vase_outline(cx,basey)
text(cx,basey-13,'VUE DE FACE',10,navy,'center')
dim_v(cx-110*scale,basey,basey+500*scale,'500')
dim_h(cx,basey-20,cx-72.5*scale,cx+72.5*scale,'Ø145')
dim_h(cx,basey+150*scale+12,cx-80*scale,cx+80*scale,'Ø160 MAX')
dim_h(cx,basey+500*scale+12,cx-45*scale,cx+45*scale,'Ø90')
text(cx,basey+305*scale,'Début du resserrement',7,grey,'center')

# section
cx2=225*mm
vase_outline(cx2,basey,True)
text(cx2,basey-13,'COUPE AXIALE A-A',10,navy,'center')
dim_h(cx2,basey+500*scale+25,cx2-45*scale,cx2+45*scale,'Ø90 ext.')
dim_h(cx2,basey+500*scale+38,cx2-42*scale,cx2+42*scale,'Ø84 int.')
line(cx2+42*scale,basey+500*scale,cx2+68*scale,basey+500*scale,blue,.5)
text(cx2+71*scale,basey+500*scale-2,'Lèvre R1.5',8,blue)
line(cx2-69.5*scale,basey+5*scale,cx2-105*scale,basey+5*scale,blue,.5)
text(cx2-109*scale,basey+5*scale-2,'Fond 5',8,blue,'right')
line(cx2+64*scale,basey+380*scale,cx2+98*scale,basey+380*scale,blue,.5)
text(cx2+101*scale,basey+380*scale-2,'Paroi 3',8,blue)

# Profile data table
x=285*mm; y=H-48*mm
text(x,y,'PROFIL EXTERIEUR - POINTS DE CONTROLE',10,navy); y-=8*mm
c.setFillColor(light); c.rect(x,y-8*mm,120*mm,8*mm,fill=1,stroke=0)
text(x+8*mm,y-5*mm,'Z (mm)',8); text(x+50*mm,y-5*mm,'R ext. (mm)',8)
for z,r in [(0,72.5),(20,75),(80,78),(150,80),(220,79),(300,75),(380,67),(440,55),(500,45)]:
    y-=7*mm; line(x,y,x+120*mm,y,light,.35); text(x+10*mm,y-5*mm,str(z),8); text(x+55*mm,y-5*mm,str(r),8)

# notes
nx=285*mm; ny=88*mm
text(nx,ny,'NOTES DE FABRICATION',10,navy); ny-=8*mm
notes=['1. Révolution 360° du profil autour de l’axe central.','2. Profil extérieur : spline Fit Point continue.','3. Parois nominales : 3 mm. Fond fermé : 5 mm.','4. Congé extérieur au pied : R5. Lèvre : R1.5.','5. Impression verticale, sans support; vérifier l’étanchéité.','6. Export compatible STL ou 3MF.']
for n in notes: text(nx,ny,n,8,grey); ny-=6*mm

# title block
c.setStrokeColor(navy); c.setLineWidth(.7); c.rect(W-165*mm,12*mm,145*mm,25*mm,fill=0,stroke=1)
text(W-160*mm,28*mm,'VASE DECORATIF MODERNE',10,navy)
text(W-160*mm,20*mm,'Plan de définition - modèle lisse par révolution',8,grey)
text(W-160*mm,15*mm,'Matériau conseillé : PLA / PETG | Tolérance générale : ±0.5 mm',7,grey)
c.showPage(); c.save()
print(OUT)
