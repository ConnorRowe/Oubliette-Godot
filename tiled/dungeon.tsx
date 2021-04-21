<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.5" tiledversion="1.5.0" name="Dungeon" tilewidth="16" tileheight="16" tilecount="15" columns="3">
 <image source="../textures/other_tiles/dungeon.png" width="48" height="80"/>
 <tile id="0">
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="0" y="16">
    <polyline points="0,0 0,-16 16,-16"/>
   </object>
   <object id="2" name="area" type="area" x="0" y="14" width="16" height="2"/>
  </objectgroup>
 </tile>
 <tile id="1">
  <objectgroup draworder="index" id="2">
   <object id="1" name="area" type="area" x="0" y="14" width="16" height="2"/>
   <object id="2" name="occluder" type="occluder" x="0" y="0">
    <polyline points="0,0 16,0"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="2">
  <objectgroup draworder="index" id="2">
   <object id="2" name="occluder" type="occluder" x="0" y="0">
    <polyline points="0,0 16,0 16,16"/>
   </object>
   <object id="4" name="area" type="area" x="0" y="14" width="16" height="2"/>
  </objectgroup>
 </tile>
 <tile id="3">
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="0" y="0">
    <polyline points="0,0 0,16"/>
   </object>
   <object id="3" name="area" type="area" x="0" y="0" width="4" height="16"/>
  </objectgroup>
 </tile>
 <tile id="5">
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="16" y="0">
    <polyline points="0,0 0,16"/>
   </object>
   <object id="2" name="area" type="area" x="12" y="0" width="4" height="16"/>
  </objectgroup>
 </tile>
 <tile id="6">
  <objectgroup draworder="index" id="2">
   <object id="4" name="occluder" type="occluder" x="0" y="0">
    <polygon points="16,3 0,3 0,0 16,0"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="7">
  <objectgroup draworder="index" id="2">
   <object id="1" name="area" type="area" x="0" y="14" width="16" height="2"/>
   <object id="5" name="occluder" type="occluder" x="0" y="0">
    <polygon points="16,3 0,3 0,0 16,0"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="8">
  <objectgroup draworder="index" id="2">
   <object id="3" name="occluder" type="occluder" x="0" y="0">
    <polygon points="16,3 0,3 0,0 16,0"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="9">
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="16" y="0">
    <polyline points="0,0 0,16"/>
   </object>
   <object id="2" name="area" type="area" x="12" y="0" width="4" height="16"/>
  </objectgroup>
 </tile>
 <tile id="10">
  <objectgroup draworder="index" id="2">
   <object id="1" name="area" type="area" x="0" y="14" width="16" height="2"/>
   <object id="2" name="occluder" type="occluder" x="0" y="0">
    <polygon points="16,3 0,3 0,0 16,0"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="11">
  <objectgroup draworder="index" id="2">
   <object id="1" name="occluder" type="occluder" x="0" y="0">
    <polyline points="0,0 0,16"/>
   </object>
   <object id="2" name="area" type="area" x="0" y="0" width="4" height="16"/>
  </objectgroup>
 </tile>
 <tile id="12" type="a">
  <objectgroup draworder="index" id="2">
   <object id="1" name="area" type="area" x="0" y="0" width="16" height="16"/>
  </objectgroup>
 </tile>
 <tile id="13">
  <objectgroup draworder="index" id="2">
   <object id="1" name="area" type="area" x="0" y="0" width="16" height="16"/>
  </objectgroup>
 </tile>
</tileset>
