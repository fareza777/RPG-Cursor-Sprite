# EMBERWAKE — Grand Story Bible
### Top-down Action Adventure · Target tamat 20–30 jam · Play Store

> Versi 1.0 · 17 Sep 2026  
> Genre: #1 Action Adventure (Zelda-like)  
> Engine: Unity 6000.6.1f1 URP 2D · Aset: Super Retro Collection  
> Folder project: `C:\Pixel Game Cursor`

---

## 0. Pitch satu halaman

**Judul:** Emberwake  
**Tagline:** *Ketika lentera terakhir padam, dunia lupa namanya sendiri.*

Di kerajaan **Virelia**, setiap desa hidup dari **Wick** — api suci yang menjaga memori, musim, dan batas antara dunia hidup dengan **Ashdeep** (dunia abu). Wick padam satu per satu. Pemain adalah **Kael**, penjaga lentera muda yang dianggap “mati” 7 tahun lalu — tapi tubuhnya kembali tanpa bayangan.

Game ini petualangan top-down: desa hub → overworld → 6 dungeon utama → boss → kunci item → twist identitas → ending.

**Fantasi pemain:** “Aku membuka jalan dengan pedang, busur, dan alat; aku pecahkan rahasia kenapa aku masih hidup; aku pilih apakah dunia layak diingat atau dilupakan.”

---

## 1. World bible

### 1.1 Geografi Virelia (6 region + hub)

| Region | Nama | Bioma (aset) | Dungeon utama | Wick status awal |
|---|---|---|---|---|
| Hub | **Millbrook** | Village / marketplace / indoor | — | Wick lemah, berkedip |
| R1 | **Verdant Reach** | Forest + farm | **Hollowroot Sanctum** | Padam → quest aktifkan |
| R2 | **Sunken Marches** | Water / swamp / beach water | **Tideglass Crypt** | Padam |
| R3 | **Cinder Dunes** | Desert | **Mirage Spire** | Padam |
| R4 | **Ashfall Peaks** | Mountain / lava accents | **Furnace of Names** | Padam |
| R5 | **Pale Orchard** | Autumn / indoor ruins | **Silent Gallery** | Padam |
| R6 | **Nightreach Capital** | Dungeon city / torii / statues | **Throne of Unlit Kings** | Hampir mati |
| Final | **Ashdeep** | Dark dungeon + 2D light ekstrem | **Heart of the Wick** | Sumber semua Wick |

Overworld memakai tile atlas + autotile. Tiap region punya **mini-overworld icon** (aset overworld di atlas) di peta dunia.

### 1.2 Aturan dunia (penting untuk mekanik)

1. **Wick Memory:** benda/NPC dekat Wick yang hidup punya nama & dialog penuh. Dekat Wick mati → nama jadi `???`, dialog putus, map fog tebal.
2. **Ashbleed:** malam hari, monster Ashdeep merembes ke overworld (alasan combat malam / dungeon).
3. **Lentera Kael:** item kunci cerita. Tanpa isi, dungeon gelap. Diisi ulang di altar Wick.
4. **Bayangan:** makhluk Ashdeep **tidak punya bayangan**. Kael juga tidak — petunjuk twist sejak jam 1.

---

## 2. Plot lengkap (spoiler total)

### ACT I — “Nama yang Hilang” (±0–6 jam)
Kael terbangun di pinggir Millbrook tanpa ingat 7 tahun terakhir. Kepala desa **Mara** bilang Kael “dikubur” setelah Insiden Wick. Adik Kael, **Lira**, kini remaja dan menjaga lentera desa yang sekarat.

**Inciting incident:** Wick Millbrook hampir padam. Monster menyerang. Kael menyelamatkan Lira; lentera di tangannya **menyala sendiri**. Mara mengirim Kael ke Hollowroot Sanctum untuk menyalakan Wick hutan.

**Dungeon 1 — Hollowroot Sanctum**  
Tema: akar raksasa, rumput tinggi, block dorong.  
Boss: **Barkwraith** (Monsters_01 — slime/forest titan vibe).  
Item kunci: **Gloves of Lift** (animasi lift/carry).  
Wick hutan hidup → nama-nama pohon & path terbuka.

**Mid-act beat:** Lira bilang ayah mereka (Wickkeeper lama) hilang di ibu kota, bukan mati. Mara menutup mulut.

### ACT II — “Empat Api” (±6–16 jam)
Kael harus menyalakan 4 Wick regional agar peta Nightreach terbuka.

| Urutan | Dungeon | Boss | Key Item | Tema puzzle |
|---|---|---|---|---|
| 2 | Tideglass Crypt | **Brinequeen** (Lamia/water) | **Tide Bow** | Saklar jauh, panah, air |
| 3 | Mirage Spire | **Glassjackal** (Scorpion elite) | **Mirage Shield** | Mirror path, block + shield reflect |
| 4 | Furnace of Names | **Cinder Minotaur** | **Ashbrand Sword+** | Lava, timing, spin attack |
| 5 | Silent Gallery | **Portrait Ghost** | **Echo Lantern** | Gelap, 2D light, ilusi NPC |

Di tengah Act II (setelah dungeon 3): **Plot twist A — “Kael adalah Wick berjalan.”**  
Altar Mirage menunjukkan refleksi: Kael bukan manusia penuh. Tubuhnya diganti jadi **Vessel** — wadah api. “Kematian”-nya 7 tahun lalu adalah ritual gagal yang dilakukan **Ordo Unlit** untuk menciptakan lentera hidup.

Lira marah / takut, tapi tetap ikut sebagai companion ringan di hub (bukan party combat penuh — sesuai aset 1 hero).

### ACT III — “Raja Tanpa Bayangan” (±16–24 jam)
Ibu kota Nightreach terbuka. Rakyat lupa nama raja. Patung tanpa wajah.

**Dungeon 6 — Throne of Unlit Kings**  
Boss: **King Without Shadow** (pakai Character battler / boss besar).  
Twist B: Raja adalah **ayah Kael**, **Aldren**, yang memilih jadi Unlit agar Wick dunia tidak meledak. Ia memakan memori kerajaan sebagai bahan bakar.

Kael kalahkan bentuk pertama. Aldren minta Kael **membunuhnya** dan membawa hatinya ke Ashdeep — atau bergabung jadi Unlit kedua.

### ACT IV — “Emberwake” (±24–30 jam)
**Ashdeep / Heart of the Wick** — final dungeon.  
Boss final fase 1: **Ashdeep Avatar** (Monsters elite besar).  
Fase 2: **Kael’s Shadow** — bayangan yang tidak pernah dimiliki Kael, sekarang hidup (pakai hero color_5 / tint gelap + ARPG VFX).

**Ending (3 jalur, berdasarkan pilihan + item tersembunyi):**

| Ending | Syarat | Hasil |
|---|---|---|
| **A. Dawnwick** (True) | Kumpulkan 6 **Memory Shards** + sparing Lira di final | Kael lepaskan Vessel, jadi manusia fana; Wick kecil tapi stabil; New Game+ |
| **B. Eternal Wick** | Kalahkan final tanpa shard lengkap, pilih “jadi lentera” | Dunia abadi terang; Kael hilang jadi api di langit; Lira jadi Wickkeeper |
| **C. Ash Quiet** | Terima tawaran Unlit di Throne | Dunia gelap tenang; monster hilang; nama semua orang terhapus; ending horor sunyi |

Syarat true ending mendorong side content → total 20–30 jam.

---

## 3. Karakter

### 3.1 Playable

#### KAEL — Protagonis (Hero color_1 default)
- **Umur tampilan:** ±19 · **Sebenarnya:** tubuh Vessel berusia 7 tahun ritual  
- **Peran:** Wick Vessel / penjelajah  
- **Kepribadian:** pendiam, praktis, humor kering; takut dekat orang karena “tidak punya bayangan”  
- **Arc:** dari “cari ingatanku” → “pilih apakah dunia perlu ingat”  
- **Combat identity:** pedang + item kunci dungeon (bow, shield, lift, lantern)  
- **Skin/color:** color_1 story default; color_2–5 unlock (armor/ritual tiers / IAP opsional)

### 3.2 Companion & hub cast

| Nama | Peran | Sheet aset | Arc singkat |
|---|---|---|---|
| **Lira** | Adik; aspirasi Wickkeeper | chara khusus / color_2 tint | Dari bergantung → pemimpin Millbrook |
| **Mara** | Kepala desa | chara_02 | Tahu ritual; bohong demi desa; reveal Act II |
| **Tobin** | Pandai besi / shop | chara_05 | Upgrade senjata; quest temper |
| **Sera** | Penjaga perpustakaan | chara_08 | Lore dump + Memory Shard hints |
| **Rook** | Pemburu swamp | chara_12 | Guide Marches; bisa mati jika side gagal |
| **Nessa** | Pedagang keliling | chara_15 | Shop mobile; rumor region |
| **Father Aldren** | Ayah / Raja Unlit | Character_01 battler + NPC cloaked | Antagonis tragis |
| **The Chorus** | Suara Ashdeep | — (UI text only) | Bisikan di dungeon gelap |
| **Warden Elira** | Kapten Nightreach | chara_20 | Rival → ally Act III |
| **Ordo Unlit (mask)** | Kultus | Monsters / Ghost frames | Musuh story berulang |

### 3.3 Boss roster (mapping aset)

| Boss | Aset usulan | Dungeon | Mechanic ringkas |
|---|---|---|---|
| Barkwraith | Monsters_01 | Hollowroot | Root slam; potong akar dengan sword |
| Brinequeen | Lamia battler + Monsters water | Tideglass | Tide phases; shoot weak point dengan bow |
| Glassjackal | Scorpion + desert | Mirage | Clone mirrors; shield bash clones |
| Cinder Minotaur | MinotaurA/B | Furnace | Charge; spin i-frame; lava ring |
| Portrait Ghost | Ghost A–D + Gallery | Silent | Hanya terlihat saat lantern on |
| King Without Shadow | Character_01–04 / big | Throne | Steal player item temporarily |
| Ashdeep Avatar | Monsters_05 / bonus | Final | Multi-phase arena |
| Kael’s Shadow | Hero tint + ARPG slash | Final P2 | Mirror moveset pemain |

### 3.4 Musuh reguler (overworld / dungeon)

Slime, Slimesword, Skeleton, Skeletonwarrior, Zombi, Mushroom, Wasp, Worm, Ghost — dari `Battlers/` + `Characters/Monsters/` untuk versi top-down.  
Tiap region 3–4 tipe + 1 elite.

---

## 4. Item & progression equipment

### 4.1 Key items (story gate — wajib)

| Item | Dungeon | Membuka |
|---|---|---|
| **Gloves of Lift** | D1 | Angkat pot/batu, pressure plate berat |
| **Tide Bow** | D2 | Panah saklar, musuh terbang |
| **Mirage Shield** | D3 | Block, reflect beam, shield_walk |
| **Ashbrand** (sword upgrade) | D4 | Potong vine besi / damage boss fire |
| **Echo Lantern** | D5 | Terang dungeon, ungkap ilusi, isi Wick |
| **King’s Sigil** | D6 | Buka gerbang Ashdeep |
| **Heartwick** | Final | Ending branch item |

### 4.2 Senjata & tools (upgrade path)

| Slot | Tier 0 | Tier 1 | Tier 2 | Tier 3 |
|---|---|---|---|---|
| Sword | Traveler Blade | Ashbrand | Wickfang | Dawnblade (true) |
| Bow | — | Tide Bow | Stormstring | — |
| Shield | Wood Buckler | Mirage Shield | King Guard | — |
| Lantern | Village Spark | Echo Lantern | Heartwick | — |

Upgrade di Tobin: material dungeon + gold.

### 4.3 Consumables

- **Heart Drop** (heal 1 heart)  
- **Wick Oil** (lantern fuel)  
- **Ash Salt** (buff resist ghost 60s)  
- **Potion of Firm Foot** (no knockback singkat)  
- **Memory Tea** (ungkap 1 dialog tersembunyi NPC)

### 4.4 Collectibles (untuk 20–30 jam)

| Koleksi | Jumlah | Reward |
|---|---|---|
| **Heart Containers** | 12 (max HP 20) | Tankiness |
| **Memory Shards** | 6 (1 per region) | True ending |
| **Wick Crests** | 20 | Map unlock / cosmetics |
| **Secret Poems** | 10 | Lore codex |
| **Pig Charms** (side farm Millbrook) | 6 | Lucu + minor luck |

### 4.5 Inventory rules
- Key items: tidak bisa dibuang  
- Hotbar mobile: Sword / Bow / Shield / Lantern / Bomb(opsional later) / Lift  
- Max inventory stack consumable: 20  

---

## 5. Sistem leveling & stats

Bukan XP RPG klasik murni — hybrid **Zelda hearts + soft levels** agar cocok action adventure tapi terasa “panjang”.

### 5.1 Player stats

| Stat | Sumber | Cap |
|---|---|---|
| **Hearts (HP)** | Heart Containers + boss | 3 → 20 |
| **Stamina** | Stamina vessels (8) | 50 → 130 |
| **Sword Power** | Weapon tier + temper | 1.0 → 2.5× |
| **Bow Power** | Bow tier | — |
| **Defense** | Shield tier + tunic color | 0 → 40% dmg reduce |
| **Lantern Radius** | Lantern tier | 3 → 10 tiles |
| **Wick Affinity** | Story flags + shards | Ending & damage vs Ash |

### 5.2 Soft level: “Wick Rank”

Setiap 8 musuh / puzzle penting / quest = **1 Wick Essence**.  
100 Essence = **+1 Wick Rank** (max Rank 30).

Per rank bonus kecil (pilih 1 saat level-up — ala minor RPG):
- +2 max stamina  
- +3% sword  
- +3% bow  
- +1% crit (max 15%)  
- +heart fragment (4 fragment = 1 container) — limited

Ini mengisi jam play tanpa wajib grinding jika pemain explorasi normal.

### 5.3 Enemy scaling
Region level band:

| Region | Enemy HP band | Damage |
|---|---|---|
| Verdant | 2–4 hits | 0.5 heart |
| Marches | 3–5 | 0.5–1 |
| Dunes | 4–6 | 1 |
| Peaks | 5–7 | 1 |
| Orchard | 5–8 ghost resist | 1 |
| Capital | 6–10 | 1–1.5 |
| Ashdeep | 8–14 | 1.5–2 |

Boss selalu telegraph jelas; mobile-friendly i-frame setelah hit.

### 5.4 Skill unlocks (bukan skill tree berat)

| Jam kira-kira | Unlock |
|---|---|
| Prolog | Walk, sword basic |
| D1 clear | Lift, spin attack (stamina) |
| D2 | Bow aim, charged shot |
| D3 | Perfect shield (parry window) |
| D4 | Sword beam saat full HP |
| D5 | Lantern pulse (stun ghost) |
| D6 | Sigil warp ke altar |
| Post-true | New Game+ keep ranks |

---

## 6. Struktur konten & estimasi jam

### 6.1 Main path (~14–18 jam)

| Segmen | Jam |
|---|---|
| Prolog + Millbrook tutorial | 0.5–1 |
| Overworld R1 + D1 | 2–2.5 |
| R2 + D2 | 2–2.5 |
| R3 + D3 + Twist A cutscene | 2.5–3 |
| R4 + D4 | 2–2.5 |
| R5 + D5 | 2–2.5 |
| Capital + D6 + Twist B | 2–3 |
| Ashdeep + Final + Ending | 2–3 |

### 6.2 Side content (~8–12 jam) — wajib untuk 20–30 jam

| Side | Jam | Isi |
|---|---|---|
| Millbrook farm mini-quests | 1.5 | Tanam 3 crop story, babi Lira |
| 12 NPC heart quests | 3–4 | Hadiah shard hints / items |
| 6 mini-dungeons (1 per region) | 3–4 | Heart containers |
| Collection hunt (crests/poems) | 2–3 | Lore + true ending |
| Optional superboss **Genius** | 0.5–1 | Post-D6 |

**Target desain:** main-only ending B/C ≈ 16–18 jam; true ending A ≈ 24–28 jam; completionist ≈ 30–35 jam.

### 6.3 Dungeon template (standar kualitas)

Tiap dungeon utama:
- 12–18 rooms  
- 1 mini-boss mid  
- 3 puzzle types (block, switch, item-gate)  
- 1 dark room / 1 combat gauntlet / 1 secret  
- Map pickup di room 4–6  
- Boss + key item + Wick altar  

---

## 7. Quest list ringkas

### Main quests (MQ)
1. Ashes at Dawn — bangun di Millbrook  
2. Keep the Village Wick  
3. Roots of Hollowroot  
4. Tideglass Awakening  
5. Mirage That Speaks Truth *(Twist A)*  
6. Names in the Furnace  
7. Gallery of the Forgotten  
8. March on Nightreach  
9. Throne Without Shadow *(Twist B)*  
10. Emberwake *(Final + endings)*

### Side quests unggulan (SQ)
- SQ01 Lira’s First Harvest  
- SQ02 Tobin’s Broken Temper  
- SQ03 Rook’s Last Hunt *(fail state)*  
- SQ04 Sera’s Forbidden Page → Memory Shard hint  
- SQ05 The Pig That Remembered Its Name  
- SQ06 Warden’s Duel  
- SQ07 Poems for the Unnamed  
- SQ08 Nessa’s Debt in Dunes  
- SQ09 Ghost Market (malam Millbrook)  
- SQ10 Father’s Letter (pre-final unlock)

---

## 8. Tema, tone, twist ringkas (untuk marketing)

**Tema:** memori vs ketenangan; cinta keluarga vs kebenaran; apakah lupa adalah belas kasih.  
**Tone:** cozy village ↔ dungeon gelap; pixel warm by day, 2D light horror by night.  
**Twist marketing-safe:** “Kamu bukan hero yang kembali — kamu api yang belum padam.”

---

## 9. Mapping ke aset Super Retro (implementasi)

| Kebutuhan story | Aset |
|---|---|
| Kael | `Hero/hero/color_1…5` semua verb |
| Panah | `Hero/arrow` |
| Training | `Hero/dummy` |
| NPC | `Characters/chara_*` + `chara_01…03` |
| Boss besar | `Characters/Monsters/Monsters_0N` |
| JRPG portrait / alternate | `Battlers/*` |
| Skill VFX | `ARPG/character_*` slash/spear/staff |
| Desa / farm / market | Samples + Prefabs Houses/Crops |
| Puzzle | `Prefabs_with_behavior` chest/block/grass |
| Dunia | `Environments` autotile + palettes |
| Battle BG cutscene | `Backgrounds` 320×240 |

**Harus dibuat sendiri:** audio, UI/HUD, dialog system, save, AI, camera room-lock, Animator Hero.

---

## 10. Monetisasi Play Store (selaras story)

- **Base game:** full story ending B/C  
- **Cosmetic:** hero color_2–5 sebagai “Ritual Garb” (bisa juga unlock gratis via play)  
- **No pay-to-win** pada sword power  
- Optional tip jar / remove ads hanya di hub  

---

## 11. Definition of Done — vertical slice dulu

Sebelum full 30 jam, ship slice **Act I lengkap**:
- Millbrook hub  
- Verdant Reach overworld  
- Hollowroot Sanctum (12 room + Barkwraith)  
- Gloves of Lift  
- Save/load  
- Mobile controls  
- ±45–70 menit play  

Ini fondasi sistem untuk 5 dungeon berikutnya.

---

## 12. Glossarium nama

| Istilah | Arti |
|---|---|
| Wick | Api suci memori |
| Vessel | Manusia-wadah Wick (Kael) |
| Ashdeep | Dunia abu di balik api |
| Unlit | Kultus yang memadamkan demi “kedamaian” |
| Emberwake | Peristiwa/final: api bangkit sadar |
| Memory Shard | Pecahan nama dunia |

---

*Dokumen ini adalah sumber kebenaran narasi & progression untuk development di `C:\Pixel Game Cursor`.*
