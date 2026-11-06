"""Generate the vector icon family used by Tyanitia's menus."""
from pathlib import Path

OUT = Path(__file__).resolve().parents[1] / "textures/ui/refresh"
OUT.mkdir(parents=True, exist_ok=True)

icons = {
    "home": '<path d="m7 29 25-21 25 21v25H7Z"/><path d="M25 54V35h14v19" stroke="#9fe8dc"/>',
    "community": '<path d="M11 15h42v31H32L18 56V46h-7Z"/><path d="M22 27h20M22 35h14" stroke="#9fe8dc"/>',
    "repository": '<circle cx="17" cy="17" r="5"/><circle cx="47" cy="17" r="5"/><circle cx="32" cy="48" r="5"/><path d="M17 22v10l15 11m15-21v10L32 43" stroke="#9fe8dc"/>',
    "play": '<path d="M23 15v34l27-17Z" fill="#9fe8dc" stroke="none"/><path d="M23 15v34l27-17Z"/>',
    "autoplay": '<rect x="14" y="17" width="36" height="32" rx="9"/><path d="M21 17v-5m22 5v-5M21 36h2m18 0h2"/><path d="m28 25 13 8-13 8Z" fill="#9fe8dc" stroke="none"/>',
    "settings": '<path d="m27 9 2-4h6l2 4 6 2 4-2 4 5-2 4 3 6 5 2v7l-5 2-3 6 2 4-4 5-4-2-6 2-2 5h-6l-2-5-6-2-4 2-4-5 2-4-3-6-5-2v-7l5-2 3-6-2-4 4-5 4 2Z"/><circle cx="32" cy="30" r="8" fill="#9fe8dc" stroke="none"/>',
    "quit": '<path d="M28 10H17a5 5 0 0 0-5 5v34a5 5 0 0 0 5 5h11M36 14h11a5 5 0 0 1 5 5v26a5 5 0 0 1-5 5H36"/><path d="M23 32h25m-8-8 8 8-8 8"/>',
    "folder": '<path d="M8 18h18l6 6h24v24a5 5 0 0 1-5 5H13a5 5 0 0 1-5-5Z"/><path d="M8 25h48"/>',
    "import": '<path d="M12 43v9h40v-9M32 8v31m-9-9 9 9 9-9"/>',
    "random": '<path d="M7 17h9c11 0 14 30 27 30h6m-8-8 8 8-8 8M7 47h9c7 0 10-9 14-17m5-10c3-3 6-3 9-3h5m-8-8 8 8-8 8"/>',
    "filter": '<path d="M8 12h48L38 34v17l-12 5V34Z"/>',
    "sort": '<path d="M15 15h27M15 27h21M15 39h15M47 13v35m-8-8 8 8 8-8"/>',
    "sort_ascend": '<path d="M15 15h15M15 27h21M15 39h27M47 49V14m-8 8 8-8 8 8"/>',
    "list": '<path d="M24 15h31M24 32h31M24 49h31"/><circle cx="12" cy="15" r="3" fill="#9fe8dc"/><circle cx="12" cy="32" r="3" fill="#9fe8dc"/><circle cx="12" cy="49" r="3" fill="#9fe8dc"/>',
    "grid": '<rect x="9" y="9" width="19" height="19" rx="4"/><rect x="36" y="9" width="19" height="19" rx="4"/><rect x="9" y="36" width="19" height="19" rx="4"/><rect x="36" y="36" width="19" height="19" rx="4"/>',
    "search": '<circle cx="27" cy="27" r="17"/><path d="m40 40 15 15"/>',
    "author": '<circle cx="32" cy="22" r="11"/><path d="M10 54c2-13 10-20 22-20s20 7 22 20"/>',
    "bookmark": '<path d="M17 9h30v46L32 44 17 55Z"/><path d="M25 26h14m-7-7v14" stroke="#9fe8dc"/>',
    "bookmark_filled": '<path d="M17 9h30v46L32 44 17 55Z" fill="#9fe8dc"/><path d="m24 29 6 6 11-13" stroke="#14232c"/>',
    "pause": '<rect x="18" y="13" width="9" height="38" rx="3" fill="#9fe8dc" stroke="none"/><rect x="37" y="13" width="9" height="38" rx="3" fill="#9fe8dc" stroke="none"/>',
    "skip": '<path d="m12 15 27 17-27 17Z"/><path d="M46 15v34"/>',
    "nofail": '<path d="M32 7 52 15v17c0 13-8 20-20 25-12-5-20-12-20-25V15Z"/><path d="m22 32 7 7 14-16" stroke="#9fe8dc"/>',
    "hardrock": '<path d="m8 47 12-27 12 12 12-22 12 37Z"/><path d="M9 48h46" stroke="#9fe8dc"/>',
    "ghost": '<path d="M15 53V27a17 17 0 0 1 34 0v26l-9-6-8 6-8-6Z"/><circle cx="25" cy="30" r="2.8" fill="#9fe8dc" stroke="none"/><circle cx="39" cy="30" r="2.8" fill="#9fe8dc" stroke="none"/>',
    "strobe": '<circle cx="32" cy="32" r="9" fill="#9fe8dc" stroke="none"/><path d="M32 5v13m0 28v13M5 32h13m28 0h13M13 13l9 9m20 20 9 9M51 13l-9 9M22 42l-9 9"/>',
    "chaos": '<path d="M9 16h8c13 0 13 32 29 32h9m-8-8 8 8-8 8M9 48h8c6 0 10-6 13-12m5-11c3-5 6-9 11-9h9m-8-8 8 8-8 8"/>',
    "vortex": '<path d="M32 31c5-6 11-3 11 3 0 9-9 14-18 11-13-4-15-20-5-29C35 3 58 13 56 33c-2 21-22 30-40 21"/>',
    "earthquake": '<path d="M5 34h11l7-17 9 34 7-21 5 7h15"/>',
    "hflip": '<path d="M32 9v46M25 17 9 32l16 15m14-30 16 15-16 15"/><path d="M14 32h13m10 0h13" stroke="#9fe8dc"/>',
    "vflip": '<path d="M9 32h46M17 25 32 9l15 16M17 39l15 16 15-16"/><path d="M32 14v13m0 10v13" stroke="#9fe8dc"/>',
}

for name, body in icons.items():
    svg = f'''<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64"><g fill="none" stroke="#edf2f7" stroke-width="3.6" stroke-linecap="round" stroke-linejoin="round">{body}</g></svg>\n'''
    (OUT / f"{name}.svg").write_text(svg)
