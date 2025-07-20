using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Linq;

public class EntitiesTool : EditorWindow
{
  private string pathPrefix = "C:/temp/entities.json";
  private int maxHierarchyLevel = -1; // -1 means unlimited depth

  [MenuItem("Tools/Entities Tool")]
  public static void ShowWindow()
  {
    GetWindow<EntitiesTool>("Entities Tool");
  }

  private void OnGUI()
  {
    GUILayout.Label("Entities Tool Settings", EditorStyles.boldLabel);

    pathPrefix = EditorGUILayout.TextField("Save File", pathPrefix);
    
    GUILayout.Space(10);
    
    GUILayout.Label("Hierarchy Depth Settings", EditorStyles.boldLabel);
    
    // Create a nice UI for max hierarchy level
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label("Max Hierarchy Level:", GUILayout.Width(140));
    
    int newMaxLevel = EditorGUILayout.IntField(maxHierarchyLevel, GUILayout.Width(50));
    
    // Clamp to reasonable values
    if (newMaxLevel < -1)
      newMaxLevel = -1;
    if (newMaxLevel > 50) // Prevent stack overflow with reasonable limit
      newMaxLevel = 50;
    
    maxHierarchyLevel = newMaxLevel;
    
    if (maxHierarchyLevel == -1)
    {
      GUILayout.Label("(Unlimited)", EditorStyles.miniLabel);
    }
    else
    {
      GUILayout.Label($"(0-{maxHierarchyLevel} levels)", EditorStyles.miniLabel);
    }
    
    EditorGUILayout.EndHorizontal();
    
    // Quick preset buttons
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label("Quick Presets:", GUILayout.Width(90));
    
    if (GUILayout.Button("Unlimited", GUILayout.Width(70)))
      maxHierarchyLevel = -1;
    
    if (GUILayout.Button("Root Only", GUILayout.Width(70)))
      maxHierarchyLevel = 0;
      
    if (GUILayout.Button("2 Levels", GUILayout.Width(60)))
      maxHierarchyLevel = 1;
      
    if (GUILayout.Button("3 Levels", GUILayout.Width(60)))
      maxHierarchyLevel = 2;
    
    EditorGUILayout.EndHorizontal();
    
    GUILayout.Space(5);
    
    // Help text
    EditorGUILayout.HelpBox(
      "Max Hierarchy Level determines how deep the export goes:\n" +
      "• -1 or Unlimited: Export all children recursively\n" +
      "• 0 or Root Only: Export only root GameObjects (no children)\n" +
      "• 1: Export root + 1 level of children\n" +
      "• 2: Export root + 2 levels of children, etc.", 
      MessageType.Info
    );

    GUILayout.Space(10);

    if (GUILayout.Button("Export Entities from All Open Scenes"))
    {
      Export();
    }
  }

  [System.Serializable]
  public class EntityNode
  {
    public string name;
    public string scene;
    public string type; // Added entity type
    public int hierarchyLevel;
    public float[] position;
    public string[] components; // Added component list
    public List<EntityNode> children = new List<EntityNode>();
  }

  [System.Serializable]
  public class EntityCollection
  {
    public List<EntityNode> items = new List<EntityNode>();
    public int maxHierarchyLevel;
    public string exportedAt;
    public string exportedBy;
    public Dictionary<string, int> typeStats = new Dictionary<string, int>(); // Statistics of entity types
  }

  [System.Serializable]
  private class Wrapper<T> { public T[] Items; }

  public static string ToJson<T>(List<T> list, bool pretty = false)
  {
    var wrapper = new Wrapper<T> { Items = list.ToArray() };
    return JsonUtility.ToJson(wrapper, pretty);
  }

  void Export()
  {
    EntityCollection collection = new EntityCollection
    {
      maxHierarchyLevel = this.maxHierarchyLevel,
      exportedAt = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
      exportedBy = System.Environment.UserName
    };

    int totalEntities = 0;
    Dictionary<string, int> typeCount = new Dictionary<string, int>();

    // Iterate through all loaded scenes
    for (int i = 0; i < SceneManager.sceneCount; i++)
    {
      Scene scene = SceneManager.GetSceneAt(i);
      
      // Skip unloaded scenes
      if (!scene.isLoaded)
        continue;

      Debug.Log($"Processing scene: {scene.name} (Max Level: {(maxHierarchyLevel == -1 ? "Unlimited" : maxHierarchyLevel.ToString())})");

      // Get all root GameObjects from the current scene
      foreach (GameObject go in scene.GetRootGameObjects())
      {
        EntityNode root = SerializeTransform(go.transform, scene.name, 0);
        collection.items.Add(root);
        totalEntities += CountEntitiesInNode(root);
        CountEntityTypes(root, typeCount);
      }
    }

    // Add type statistics to collection
    collection.typeStats = typeCount;

    string json = JsonUtility.ToJson(collection, true);
    File.WriteAllText(pathPrefix, json);
    
    string levelText = maxHierarchyLevel == -1 ? "unlimited levels" : $"max {maxHierarchyLevel + 1} level(s)";
    Debug.Log($"Exported {totalEntities} entities ({levelText}) from {SceneManager.sceneCount} scenes to: {pathPrefix}");
    
    // Log type statistics
    Debug.Log("Entity Type Statistics:");
    foreach (var kvp in typeCount.OrderByDescending(x => x.Value))
    {
      Debug.Log($"  {kvp.Key}: {kvp.Value} entities");
    }
    
    // Log scene summary
    for (int i = 0; i < SceneManager.sceneCount; i++)
    {
      Scene scene = SceneManager.GetSceneAt(i);
      if (scene.isLoaded)
      {
        int sceneEntityCount = 0;
        foreach (var item in collection.items)
        {
          if (item.scene == scene.name)
            sceneEntityCount += CountEntitiesInNode(item);
        }
        Debug.Log($"Scene '{scene.name}': {sceneEntityCount} total entities");
      }
    }
  }

  EntityNode SerializeTransform(Transform t, string sceneName, int currentLevel)
  {
    GameObject go = t.gameObject;
    
    EntityNode node = new EntityNode
    {
      name = t.name,
      scene = sceneName,
      type = DetermineEntityType(go),
      hierarchyLevel = currentLevel,
      position = new float[] { t.position.x, t.position.y, t.position.z },
      components = GetComponentNames(go)
    };

    // Only process children if we haven't exceeded the max level
    // maxHierarchyLevel == -1 means unlimited depth
    if (maxHierarchyLevel == -1 || currentLevel < maxHierarchyLevel)
    {
      foreach (Transform child in t)
      {
        if (child.gameObject.activeInHierarchy)
        {
          node.children.Add(SerializeTransform(child, sceneName, currentLevel + 1));
        }
      }
    }

    return node;
  }

  string DetermineEntityType(GameObject go)
  {
    // Determine entity type based on components and characteristics
    
    // Check for UI elements first
    /*
    if (go.GetComponent<Canvas>()) return "UI_Canvas";
    if (go.GetComponent<UnityEngine.UI.Button>()) return "UI_Button";
    if (go.GetComponent<UnityEngine.UI.Text>()) return "UI_Text";
    if (go.GetComponent<UnityEngine.UI.Image>()) return "UI_Image";
    if (go.GetComponent<UnityEngine.UI.ScrollRect>()) return "UI_ScrollRect";
    if (go.GetComponent<UnityEngine.UI.InputField>()) return "UI_InputField";
    if (go.GetComponent<UnityEngine.UI.Slider>()) return "UI_Slider";
    if (go.GetComponent<UnityEngine.UI.Toggle>()) return "UI_Toggle";
    */
    
    // Check for rendering components
    if (go.GetComponent<MeshRenderer>())
    {
      if (go.GetComponent<MeshFilter>())
        return "Mesh";
      else
        return "Renderer";
    }
    
    if (go.GetComponent<SkinnedMeshRenderer>()) return "SkinnedMesh";
    if (go.GetComponent<SpriteRenderer>()) return "Sprite";
    if (go.GetComponent<LineRenderer>()) return "Line";
    if (go.GetComponent<TrailRenderer>()) return "Trail";
    
    // Check for lighting
    if (go.GetComponent<Light>()) return "Light";
    if (go.GetComponent<ReflectionProbe>()) return "ReflectionProbe";
    if (go.GetComponent<LightProbeGroup>()) return "LightProbeGroup";
    
    // Check for physics
    if (go.GetComponent<Rigidbody>()) return "RigidBody";
    if (go.GetComponent<Rigidbody2D>()) return "RigidBody2D";
    if (go.GetComponent<Collider>()) return "Collider";
    if (go.GetComponent<Collider2D>()) return "Collider2D";
    
    // Check for audio
    if (go.GetComponent<AudioSource>()) return "AudioSource";
    if (go.GetComponent<AudioListener>()) return "AudioListener";
    
    // Check for particles
    if (go.GetComponent<ParticleSystem>()) return "ParticleSystem";
    
    // Check for animation
    if (go.GetComponent<Animator>()) return "Animator";
    if (go.GetComponent<Animation>()) return "Animation";
    
    // Check for cameras
    if (go.GetComponent<Camera>()) return "Camera";
    
    // Check for terrain
    if (go.GetComponent<Terrain>()) return "Terrain";
    if (go.GetComponent<TerrainCollider>()) return "TerrainCollider";
    
    // Check for special Unity objects
    if (go.GetComponent<WindZone>()) return "WindZone";
    if (go.GetComponent<OcclusionArea>()) return "OcclusionArea";
    if (go.GetComponent<LODGroup>()) return "LODGroup";
    
    // Check if it's just a container/group
    if (go.transform.childCount > 0 && go.GetComponents<Component>().Length <= 1) // Only has Transform
      return "Container";
    
    // Check for custom scripts
    Component[] components = go.GetComponents<Component>();
    bool hasCustomScript = false;
    foreach (Component comp in components)
    {
      if (comp != null && !(comp is Transform) && !comp.GetType().Namespace.StartsWith("UnityEngine"))
      {
        hasCustomScript = true;
        break;
      }
    }
    
    if (hasCustomScript) return "CustomScript";
    
    // Default fallback
    return "GameObject";
  }

  string[] GetComponentNames(GameObject go)
  {
    Component[] components = go.GetComponents<Component>();
    List<string> componentNames = new List<string>();
    
    foreach (Component comp in components)
    {
      if (comp != null)
      {
        // Get the actual type name, not just "Component"
        string typeName = comp.GetType().Name;
        
        // Skip Transform as it's always present
        if (typeName != "Transform")
        {
          componentNames.Add(typeName);
        }
      }
    }
    
    return componentNames.ToArray();
  }

  // Helper method to count total entities in a node tree
  int CountEntitiesInNode(EntityNode node)
  {
    int count = 1; // Count this node
    foreach (var child in node.children)
    {
      count += CountEntitiesInNode(child);
    }
    return count;
  }

  // Helper method to count entity types
  void CountEntityTypes(EntityNode node, Dictionary<string, int> typeCount)
  {
    if (!typeCount.ContainsKey(node.type))
      typeCount[node.type] = 0;
    typeCount[node.type]++;
    
    foreach (var child in node.children)
    {
      CountEntityTypes(child, typeCount);
    }
  }
}
