<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.4" tiledversion="1.4.3" name="BuildingsNStuff" tilewidth="16" tileheight="16" tilecount="6" columns="0">
 <grid orientation="orthogonal" width="1" height="1"/>
 <tile id="1">
  <image width="16" height="16" source="../textures/other_tiles/brickwall.png"/>
  <objectgroup draworder="index" id="2">
   <object id="1" name="area" type="area" x="0" y="14" width="16" height="2"/>
   <object id="4" name="occluder" type="occluder" x="0" y="0">
    <polyline points="0,0 16,0"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="2">
  <image width="16" height="16" source="../textures/other_tiles/brick_vert_L.png"/>
 </tile>
 <tile id="3">
  <image width="16" height="16" source="../textures/other_tiles/brick_vert_R.png"/>
 </tile>
 <tile id="4">
  <image width="16" height="16" source="../textures/other_tiles/brick_vert_top_L.png"/>
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="0" y="0">
    <polygon points="0,0 4,0 4,16 0,16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="5">
  <image width="16" height="16" source="../textures/other_tiles/brick_vert_top_R.png"/>
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="12" y="0">
    <polygon points="0,0 4,0 4,16 0,16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="6">
  <image width="16" height="16" source="../textures/other_tiles/brickdoor.png"/>
  <objectgroup draworder="index" id="2">
   <object id="2" name="area" type="area" x="0" y="14" width="3" height="2"/>
   <object id="3" name="area" type="area" x="13" y="14" width="3" height="2"/>
   <object id="4" name="area" type="area" x="13" y="14" width="3" height="2"/>
  </objectgroup>
 </tile>
</tileset>
