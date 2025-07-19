using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class ExportEntities : MonoBehaviour
{
    [System.Serializable]
    public class EntityNode
    {
        public string name;
        public float[] position;
        public List<EntityNode> children = new List<EntityNode>();
    }

    void Start()
    {
        List<EntityNode> roots = new List<EntityNode>();

        foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            roots.Add(SerializeTransform(go.transform));
        }

        string json = JsonHelper.ToJson(roots, true);

        string sceneName = SceneManager.GetActiveScene().name;
        string path = "C:/Temp" + $"/{sceneName}_entity_dump.json";

        File.WriteAllText(path, json);
        Debug.Log("Exported entity tree to: " + path);
    }

    EntityNode SerializeTransform(Transform t)
    {
        EntityNode node = new EntityNode
        {
            name = t.name,
            position = new float[] { t.position.x, t.position.y, t.position.z }
        };

        foreach (Transform child in t)
        {
            if (child.gameObject.activeInHierarchy)
                node.children.Add(SerializeTransform(child));
        }

        return node;
    }
}

