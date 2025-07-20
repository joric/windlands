import json

areas = ['jungle','desert','mountain']

features = []

max_level = 2

def add_children(p, level=0):
    if 'children' not in p:
        return
    if level==max_level:
        return
    for c in p['children']:
        if 'position' in c:
            coord = [round(x,2) for x in c['position']]
            features.append({'type': 'Feature', 'geometry': {'type': 'Point', 'coordinates': coord}, 'properties': {'name': c['name'], 'type': c['type'], 'area': area.lower()}})
        add_children(c, level+1)

for area in areas:
    for filename in [f'{area}.json']:
        j = json.load(open(filename,'r',encoding='utf8'))
        for p in j['items']:
            add_children(p)

json.dump({'type': 'FeatureCollection', 'features': features}, open('../markers.json','w'), indent=2)
