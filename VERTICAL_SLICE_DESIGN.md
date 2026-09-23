# VERTICAL SLICE — Tes HP (Portrait APK)

> Tujuan: **bukan** game penuh. Ini potongan yang terasa “game beneran” supaya kamu colok HP, main 10–20 menit, dan kasih feedback.

---

## 1. Apa yang diuji di HP

| Aspek | Pertanyaan tes |
|---|---|
| Portrait feel | Map & UI enak di layar tegak? |
| Virtual stick | Gerak akurat / kepeleset / terlalu sensitif? |
| ATK / SPIN | Tombol gampang dijangkau jempol kanan? |
| Combat | Hitbox pedang terasa adil? |
| Pacing | 10–20 menit tidak membosankan / tidak bingung? |
| Clarity | Kamu selalu tahu **mau ngapain**? |

---

## 2. Fantasy slice (satu kalimat)

**“Kael selamatkan lentera Millbrook, masuk Hollowroot, ambil Gloves, kalahkan Barkling, nyalakan Wick.”**

Selesai = layar **SLICE CLEAR** + waktu main + hearts sisa.

---

## 3. Struktur ruang (portrait = vertikal)

```
[HUB] Millbrook Yard     ← spawn, tutorial tip singkat
   ↓ gerbang utara
[R1] Root Path           ← 2 slime + tall grass feel
   ↓
[R2] Pressure Chamber    ← dorong 1 block ke plat (atau berdiri di zona)
   ↓ buka pintu
[R3] Reliquary           ← ambil Gloves of Lift (wajib)
   ↓
[R4] Barkling Nest       ← mini-boss Barkling (HP tebal, telegraph slam)
   ↓
[ALTAR] Wick Spark       ← sentuh altar = CLEAR
```

Total **5 zona**, kamera room-lock per zona (portrait: tinggi > lebar).

---

## 4. Loop gameplay yang harus kerasa

1. **Move** stick → tembak arah 4 cardinal jelas  
2. **Fight** ATK ke slime (1–3 hit)  
3. **Puzzle** ringan buka jalan  
4. **Key item** Gloves (toast + icon)  
5. **Boss** pattern sederhana (idle → charge → vulner)  
6. **Payoff** Wick menyala + CLEAR  

Kalau salah satu loncat, slice gagal sebagai tes.

---

## 5. Scope yang SENGAJA tidak masuk APK tes

- Desa penuh / NPC dialog panjang  
- Inventory UI rumit  
- Save slot banyak  
- 5 dungeon lain  
- Audio penuh (boleh 1 SFX nanti)  
- IAP / ads  

---

## 6. Metrik sukses tes

| Hasil | Arti |
|---|---|
| Selesai < 8 menit tanpa stuck | Terlalu pendek / terlalu gampang |
| Selesai 10–20 menit | **Sweet spot** |
| Stuck > 5 menit di puzzle | Puzzle jelek |
| Mati > 5× di Barkling | Boss terlalu keras / kontrol jelek |
| Tidak paham tujuan | UI objective gagal |

---

## 7. Build yang kamu install

- File: `Builds/Android/Emberwake-Slice.apk`
- Package: `com.emberwakestudio.emberwake`
- Portrait lock
- Scene: `Millbrook_Prototype` (flow slice di dalamnya)

---

## 8. Checklist saat colok HP

1. Install APK (izinkan unknown sources)  
2. Set Game tidak perlu — langsung app  
3. Main sampai CLEAR / atau catat di mana stuck  
4. Feedback ke aku: stick / tombol / boss / bingung di room mana  
