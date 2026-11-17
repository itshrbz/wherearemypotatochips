using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using Object = UnityEngine.Object;

/// <summary>Reparaciones puntuales del playtest; ejecutar con Unity CLI run_script, fuera de Assets.</summary>
public static class CorreccionesTiendasMapas
{
    const string ReplicaPath = "Assets/02_Prefabs/ReplicaStore.prefab";
    static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/03_Materials/" + name + ".mat");
    static Transform Find(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).First(t => t.name == name);
    static Scene Open(string name) => EditorSceneManager.OpenScene("Assets/00_Scenes/" + name + ".unity");
    static void Save(Scene scene) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
    static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool solid = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        var renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
    static Transform Group(Transform parent, string name, Vector3 position)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false); t.localPosition = position; return t;
    }
    static void Label(Transform parent, string name, string text, Vector3 position, Vector2 size, float yaw = 0f)
    {
        var t = Group(parent, name, position); t.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var tmp = t.gameObject.AddComponent<TextMeshPro>();
        tmp.text = text; tmp.fontSize = 2.4f; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.85f, 0.77f, 0.47f); tmp.rectTransform.sizeDelta = size;
    }

    public static string Texts()
    {
        foreach (var id in AssetDatabase.FindAssets("t:SaleVariant", new[] { "Assets/07_data/Sales" }))
        {
            var sale = AssetDatabase.LoadAssetAtPath<SaleVariant>(AssetDatabase.GUIDToAssetPath(id));
            string model = sale.productPrefab != null ? sale.productPrefab.name : "";
            if (model == "Chocolate_B") sale.productName = "Chocolate";
            if (model == "Candy_Cane") sale.productName = "Candy Cane";
            if (model == "Cupcake") sale.productName = "Cupcake";
            if (sale.buyerName == "Old Man") sale.buyerName = "Customer";
            sale.greetingLines = sale.greetingLines.Select(s => s.Replace("Potatos", "Potatoes")).ToArray();
            sale.potatoLines = sale.potatoLines.Select(s => s.Replace("Potatos", "Potatoes")).ToArray();
            sale.afterSaleLines = sale.afterSaleLines.Select(s => s.Replace("potatos", "potatoes")).ToArray();
            EditorUtility.SetDirty(sale);
            AssetDatabase.SaveAssetIfDirty(sale);
        }
        return "Nombres de productos y compradores corregidos.";
    }

    public static string Door()
    {
        var scene = Open("02_Store");
        var door = GameObject.Find("Kit_PuertaAlmacen").transform;
        // El dintel del kit y el de la tienda ocupaban el mismo plano con alturas distintas.
        var lintel = GameObject.Find("Tienda_Generada/Paredes/FondoDintel").transform;
        lintel.localPosition = new Vector3(0f, 3.1f, -4f);
        lintel.localScale = new Vector3(2f, 0.2f, 0.2f);
        foreach (var r in door.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = r.name == "Hoja" ? Mat("Tienda_Madera") : Mat("Tienda_Metal");
        PrefabUtility.RecordPrefabInstancePropertyModifications(door.gameObject);
        foreach (var r in door.GetComponentsInChildren<Renderer>()) PrefabUtility.RecordPrefabInstancePropertyModifications(r);
        Save(scene); return "Dinteles separados y puerta integrada con los materiales de la tienda.";
    }

    public static string Replica()
    {
        var root = PrefabUtility.LoadPrefabContents(ReplicaPath);
        try
        {
            var t = root.transform;
            var existing = t.Find("Replica_Details");
            if (existing != null) return "La réplica ya tiene la reparación; se conservan su decoración y referencias.";
            var counter = Find(t, "Kit_Mostrador");
            counter.localPosition = new Vector3(-1.1f, 0.02f, -1.6f);
            Find(t, "Kit_Caja").localPosition = new Vector3(-1.9f, 1.10f, -1.6f);
            // Conserva el kit y sus vínculos, pero oculta los bloques de prueba de las góndolas.
            foreach (var g in t.Cast<Transform>().Where(c => c.name == "Kit_Gondola")) g.gameObject.SetActive(false);
            foreach (var floor in t.Cast<Transform>().Where(c => c.name == "Kit_Suelo"))
                floor.localPosition = new Vector3(floor.localPosition.x, 0.02f, floor.localPosition.z);
            foreach (var ceiling in t.Cast<Transform>().Where(c => c.name == "Kit_Techo"))
                ceiling.localPosition = new Vector3(ceiling.localPosition.x, -0.025f, ceiling.localPosition.z);
            var details = Group(t, "Replica_Details", Vector3.zero);
            for (int x = -4; x < 4; x++) for (int z = -4; z < 4; z++)
                if ((x + z) % 2 == 0) Box(details, "FloorTile", new Vector3(x + 0.5f, 0.022f, z + 0.5f), new Vector3(1f, 0.004f, 1f), Mat("Tienda_Piso2"), false);
            Box(details, "LeftSkirting", new Vector3(-3.88f, 0.145f, 0f), new Vector3(0.035f, 0.25f, 7.8f), Mat("Tienda_Zocalo"), false);
            Box(details, "RightSkirting", new Vector3(3.88f, 0.145f, 0f), new Vector3(0.035f, 0.25f, 7.8f), Mat("Tienda_Zocalo"), false);
            Box(details, "BackSkirting", new Vector3(0f, 0.145f, 3.88f), new Vector3(7.8f, 0.25f, 0.035f), Mat("Tienda_Zocalo"), false);
            Shelf(details, "LeftShelf", new Vector3(-3.55f, 0.02f, 1.1f), -90f, 3.4f);
            Shelf(details, "CenterShelf", new Vector3(-0.6f, 0.02f, 2.0f), 0f, 2.5f);
            Fridge(details, new Vector3(3.42f, 0.02f, 2.15f));
            Label(details, "StoreSign", "POTATO MART", new Vector3(0f, 2.65f, 3.87f), new Vector2(3f, 0.5f));
            Label(details, "OutOfStock", "CHIPS - SOLD OUT", new Vector3(-0.6f, 1.9f, 1.72f), new Vector2(2.3f, 0.3f));
            Find(details,"OutOfStock").GetComponent<TextMeshPro>().fontSize=1.3f;
            var clock = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(clock.GetComponent<Collider>());
            clock.name = "Clock"; clock.transform.SetParent(details, false);
            clock.transform.localPosition = new Vector3(-2.5f, 2.5f, 3.85f);
            clock.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            clock.transform.localScale = new Vector3(0.4f, 0.02f, 0.4f);
            clock.GetComponent<Renderer>().sharedMaterial = Mat("Tienda_Piso1");
            Box(details, "ClockHour", new Vector3(-2.46f, 2.52f, 3.82f), new Vector3(0.1f, 0.025f, 0.01f), Mat("Tienda_Negro"), false);
            Box(details, "ClockMinute", new Vector3(-2.5f, 2.56f, 3.82f), new Vector3(0.02f, 0.14f, 0.01f), Mat("Tienda_Negro"), false);
            var buyer = Group(details, "BuyerSpot", new Vector3(-1.1f, 1f, -0.25f));
            var checkout = root.GetComponent<ReplicaCheckout>();
            var so = new SerializedObject(checkout);
            so.FindProperty("buyerSpot").objectReferenceValue = buyer; so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, ReplicaPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return "Réplica: pasillo de caja, comprador fijo, baldosas, estantes surtidos, heladera, reloj y letreros.";
    }

    public static string Materials()
    {
        foreach(string name in new[]{"Calco_Humedad1","Calco_Humedad2","Calco_Humedad3"})
        {
            var m=Mat("Kit/Calcomanias/"+name);
            // La conservación de especular iluminaba incluso los píxeles con alfa cero.
            m.SetFloat("_BlendModePreserveSpecular",0f);
            m.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SpecularHighlights",0f);m.SetFloat("_EnvironmentReflections",0f);
            m.SetFloat("_Smoothness",0f);
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            EditorUtility.SetDirty(m);
            AssetDatabase.SaveAssetIfDirty(m);
        }
        foreach(var pair in new[]{("Tienda_Pared","Yeso"),("Tienda_Techo","TechoPlacas"),("Tienda_Madera","MaderaPintada"),("Tienda_Calle","Asfalto")})
        {
            var m=Mat(pair.Item1);
            string path="Assets/05_Textures/Kit/"+pair.Item2+"/Kit_"+pair.Item2;
            m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path+"_Color.jpg"));
            m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path+"_Normal.jpg"));
            m.SetFloat("_BumpScale",0.35f);m.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(m);
            AssetDatabase.SaveAssetIfDirty(m);
        }
        return "Humedad sin brillo rectangular; tienda con texturas existentes de yeso, techo, madera y asfalto.";
    }

    public static string Maps()
    {
        var report = new System.Text.StringBuilder();
        foreach (string name in new[] { "10_Warehouse", "11_Pools", "12_Playground" })
        {
            var scene = Open(name);
            var checkout = Object.FindAnyObjectByType<ReplicaCheckout>();
            if (checkout == null)
            {
                var anchor = GameObject.Find("ReplicaStore_Anchor").transform;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ReplicaPath), scene);
                go.transform.SetParent(anchor.parent, true);
                go.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
                checkout = go.GetComponent<ReplicaCheckout>();
            }
            Vector3 center = checkout.transform.position;
            if (name == "11_Pools")
            {
                // Conserva cada tramo exterior de los muros largos; nunca se apaga toda la fila.
                var cut = new Bounds(center + Vector3.up * 2f, new Vector3(8.22f, 8f, 8.22f));
                foreach (var r in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MeshRenderer>()).Where(r => r.name.StartsWith("Wall_")).ToArray())
                    report.Append(TrimWall(r, cut));
            }
            if (name == "10_Warehouse")
            {
                foreach(var r in checkout.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name=="Muro"))
                {
                    r.gameObject.SetActive(true);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(r.gameObject);
                }
                foreach (var r in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MeshRenderer>()).Where(r => r.name == "Muro" && !r.transform.IsChildOf(checkout.transform)).ToArray())
                {
                    Bounds b = r.bounds;
                    if (b.min.x >= center.x - 4.11f && b.max.x <= center.x + 4.11f && b.min.z >= center.z + 3.85f && b.max.z <= center.z + 4.15f)
                        r.gameObject.SetActive(false);
                }
                var existing = GameObject.Find("RoofBarriers");
                if (existing != null) Object.DestroyImmediate(existing);
                var barriers = new GameObject("RoofBarriers").transform;
                // La sala de parkour sigue abierta; estas guardas impiden pasar a los techos vecinos.
                Barrier(barriers,"ParkourWest",new Vector3(-8f,5.35f,0f),new Vector3(0.2f,5.3f,8.2f));
                Barrier(barriers,"ParkourEast",new Vector3(8f,5.35f,0f),new Vector3(0.2f,5.3f,8.2f));
                Barrier(barriers,"ParkourSouth",new Vector3(0f,5.35f,-4f),new Vector3(16.2f,5.3f,0.2f));
                Barrier(barriers,"ParkourNorth",new Vector3(0f,5.35f,4f),new Vector3(16.2f,5.3f,0.2f));
                Barrier(barriers,"OuterWest",new Vector3(-32f,5.35f,0f),new Vector3(0.2f,5.3f,64f));
                Barrier(barriers,"OuterEast",new Vector3(32f,5.35f,0f),new Vector3(0.2f,5.3f,64f));
                Barrier(barriers,"OuterSouth",new Vector3(0f,5.35f,-32f),new Vector3(64f,5.3f,0.2f));
                Barrier(barriers,"OuterNorth",new Vector3(0f,5.35f,32f),new Vector3(64f,5.3f,0.2f));
            }
            Save(scene);
            report.AppendLine(name + ": réplica y perímetro preparados.");
        }
        return report.ToString();
    }
    public static string Decals()
    {
        var scene=Open("11_Pools");
        Physics.SyncTransforms();
        const string folder="Assets/04_Models/Decals";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/04_Models","Decals");
        int count=0;
        foreach(var r in scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<MeshRenderer>()).Where(r=>r.name.StartsWith("Calco_Humedad")))
        {
            var t=r.transform;
            if(!Physics.Raycast(t.position-t.forward, t.forward,out var hit,2f,~0,QueryTriggerInteraction.Ignore))continue;
            if(!hit.collider.name.StartsWith("Column_"))continue;
            var bounds=hit.collider.bounds;
            float radius=bounds.extents.x+0.008f;
            var outward=new Vector3(t.position.x-bounds.center.x,0f,t.position.z-bounds.center.z).normalized;
            var tangent=Vector3.Cross(Vector3.up,outward);
            const int columns=16,rows=4;
            var vertices=new Vector3[(columns+1)*(rows+1)];var uv=new Vector2[vertices.Length];
            var triangles=new int[columns*rows*6];
            float width=Mathf.Min(t.lossyScale.x,0.65f);
            float height=Mathf.Min(t.lossyScale.y,bounds.size.y-0.1f);
            float centerY=Mathf.Clamp(t.position.y,bounds.min.y+height/2+0.05f,bounds.max.y-height/2-0.05f);
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                float u=(float)x/columns,v=(float)y/rows,angle=(u-0.5f)*width/radius;
                Vector3 point=bounds.center+radius*(outward*Mathf.Cos(angle)+tangent*Mathf.Sin(angle));
                point.y=centerY+(v-0.5f)*height;
                int index=y*(columns+1)+x;vertices[index]=t.InverseTransformPoint(point);uv[index]=new Vector2(u,v);
                if(x==columns || y==rows)continue;
                int k=(y*columns+x)*6;
                triangles[k]=index;triangles[k+1]=index+1;triangles[k+2]=index+columns+1;
                triangles[k+3]=index+1;triangles[k+4]=index+columns+2;triangles[k+5]=index+columns+1;
            }
            var mesh=new Mesh { name="PoolsHumidity_"+hit.collider.name+"_"+r.name };
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            string path=folder+"/"+mesh.name+".asset";
            var asset=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(asset==null) { AssetDatabase.CreateAsset(mesh,path);asset=mesh; }
            else { EditorUtility.CopySerialized(mesh,asset);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(asset); }
            var filter=r.GetComponent<MeshFilter>();filter.sharedMesh=asset;
            PrefabUtility.RecordPrefabInstancePropertyModifications(filter);AssetDatabase.SaveAssetIfDirty(asset);count++;
        }
        Save(scene);return count+" manchas ajustadas a la curvatura de los pilares, sin sobresalir por los lados.";
    }
    static void Barrier(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var t=Group(parent,name,position);
        t.gameObject.AddComponent<BoxCollider>().size=size;
        var modifier=t.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
        modifier.ignoreFromBuild=true;
    }
    public static string Bake()
    {
        var report=new System.Text.StringBuilder();
        foreach(string name in new[]{"10_Warehouse","11_Pools","12_Playground"})
        {
            var scene=Open(name);
            foreach(var surface in Object.FindObjectsByType<Unity.AI.Navigation.NavMeshSurface>())
            {
                var original=surface.navMeshData;
                string path=AssetDatabase.GetAssetPath(original);
                surface.BuildNavMesh();
                var built=surface.navMeshData;
                if(built==null)throw new Exception("No se pudo hornear "+name);
                if(original!=null && original!=built && !string.IsNullOrEmpty(path))
                {
                    // Actualiza el asset existente para conservar su GUID y todas sus referencias.
                    built.name=System.IO.Path.GetFileNameWithoutExtension(path);
                    surface.RemoveData();EditorUtility.CopySerialized(built,original);EditorUtility.SetDirty(original);AssetDatabase.SaveAssetIfDirty(original);
                    surface.navMeshData=original;surface.AddData();Object.DestroyImmediate(built);
                }
                EditorUtility.SetDirty(surface);
            }
            int triangles=UnityEngine.AI.NavMesh.CalculateTriangulation().indices.Length/3;
            if(triangles==0)throw new Exception("NavMesh vacío: "+name);
            report.AppendLine(name+": "+triangles+" triángulos, "+Object.FindObjectsByType<Unity.AI.Navigation.NavMeshLink>().Length+" enlaces existentes conservados.");
            Save(scene);
        }
        return report.ToString();
    }
    static string TrimWall(MeshRenderer wall, Bounds cut)
    {
        var b=wall.bounds;
        if(!b.Intersects(cut)) return "";
        float x0=Mathf.Max(b.min.x,cut.min.x), x1=Mathf.Min(b.max.x,cut.max.x);
        float z0=Mathf.Max(b.min.z,cut.min.z), z1=Mathf.Min(b.max.z,cut.max.z);
        if(x1<=x0 || z1<=z0) return "";
        var parts=new System.Collections.Generic.List<Bounds>();
        void Part(float minX,float maxX,float minZ,float maxZ)
        {
            if(maxX-minX<0.01f || maxZ-minZ<0.01f) return;
            var part=new Bounds();part.SetMinMax(new Vector3(minX,b.min.y,minZ),new Vector3(maxX,b.max.y,maxZ));parts.Add(part);
        }
        Part(b.min.x,x0,b.min.z,b.max.z); Part(x1,b.max.x,b.min.z,b.max.z);
        Part(x0,x1,b.min.z,z0); Part(x0,x1,z1,b.max.z);
        for(int i=0;i<parts.Count;i++)
        {
            var part=parts[i];
            var go=i==0 ? wall.gameObject : Object.Instantiate(wall.gameObject,wall.transform.parent);
            go.name=wall.name+(i==0 ? "" : "_Preserved"+i);
            go.transform.position=part.center;go.transform.localScale=part.size;
        }
        if(parts.Count==0) wall.gameObject.SetActive(false);
        return wall.name+": conservados "+parts.Count+" tramos exteriores.\n";
    }

    static void Shelf(Transform parent, string name, Vector3 position, float yaw, float width)
    {
        var root = Group(parent, name, position); root.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Box(root, "Back", new Vector3(0f, 0.95f, 0.22f), new Vector3(width, 1.9f, 0.06f), Mat("Tienda_Metal"));
        Box(root, "Left", new Vector3(-width/2, 0.95f, 0f), new Vector3(0.06f, 1.9f, 0.5f), Mat("Tienda_Metal"));
        Box(root, "Right", new Vector3(width/2, 0.95f, 0f), new Vector3(0.06f, 1.9f, 0.5f), Mat("Tienda_Metal"));
        for(int row=0; row<4; row++)
        {
            float y=0.15f+row*0.47f;
            Box(root,"Shelf",new Vector3(0f,y,0f),new Vector3(width,0.035f,0.5f),Mat("Tienda_Metal"),false);
            for(int col=0; col<Mathf.FloorToInt(width/0.3f)-1;col++)
            {
                float x=-width/2+0.25f+col*0.3f;
                string[] colors={"Rojo","Amarillo","Azul","Verde","Naranja","Blanco"};
                Box(root,"Product",new Vector3(x,y+0.15f,-0.02f),new Vector3(0.2f,0.26f,0.22f),Mat("Tienda_Prod"+colors[(col+row*2)%colors.Length]),false);
            }
        }
    }
    static void Fridge(Transform parent, Vector3 position)
    {
        var t=Group(parent,"Fridge",position); t.localRotation=Quaternion.Euler(0f,90f,0f);
        Box(t,"Body",new Vector3(0f,1.1f,0f),new Vector3(1.2f,2.2f,0.7f),Mat("Tienda_Negro"));
        Box(t,"Interior",new Vector3(0f,1.1f,-0.36f),new Vector3(1.05f,1.9f,0.025f),Mat("Tienda_Piso1"),false);
        for(int row=0;row<4;row++) for(int col=0;col<4;col++)
            Box(t,"Drink",new Vector3(-0.36f+col*0.24f,0.45f+row*0.45f,-0.4f),new Vector3(0.1f,0.28f,0.12f),Mat("Tienda_ProdVerde"),false);
        Box(t,"Glass",new Vector3(0f,1.1f,-0.48f),new Vector3(1.1f,1.95f,0.02f),Mat("Tienda_Vidrio"),false);
        Box(t,"Handle",new Vector3(0.43f,1.1f,-0.51f),new Vector3(0.035f,0.6f,0.04f),Mat("Tienda_Metal"),false);
    }
}
