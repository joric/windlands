import json

areas = ['Jungle','Desert','Mountain']

features = []

def add_children(p):
    if 'children' not in p:
        return
    for c in p['children']:
        marker_type = next((k for k,v in {'relic':f'{area}_Relic', 'checkpoint': 'Checkpoint', 'secret': 'Secret'}.items() if c['name'].startswith(v)), 'other')

        if marker_type and 'position' in c:
            coord = [round(x,2) for x in c['position']]
            title = c['name']
            features.append({'type': 'Feature', 'geometry': {'type': 'Point', 'coordinates': coord}, 'properties': {'type': marker_type, 'title':title, 'area': area.lower()}})
        add_children(c)

for area in areas:
    for filename in [f'{area.lower()}_collectables_entity_dump.json', f'Windlands_{area}_entity_dump.json']:
        j = json.load(open(filename,'r',encoding='utf8'))
        for p in j['Items']:
            add_children(p)

json.dump({'type': 'FeatureCollection', 'features': features}, open('../markers.json','w'), indent=2)
