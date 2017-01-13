using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using glTFLoader;
using glTFLoader.Schema;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

public class SceneLoader
{
    public static void LoadGltfModel(Gltf model)
    {
        SceneLoader loader = new SceneLoader(model);
        loader.Load();
    }

    private Gltf m_model;
    private SceneLoader(Gltf model)
    {
        m_model = model;
    }

    private void Load()
    {
        string sceneID = m_model.Scene;
        LoadScene(m_model.Scenes[sceneID]);
    }

    private void LoadScene(Scene scene)
    {
        foreach (string nodeID in scene.Nodes)
            LoadNode(m_model.Nodes[nodeID], Matrix4x4.identity);
    }

    private void LoadNode(Node node, Matrix4x4 parentMatrix)
    {
        Vector3 translation = ArrayConverter.ToVector3(node.Translation);
        Quaternion rotation = ArrayConverter.ToQuaternion(node.Rotation);
        Vector3 scale = ArrayConverter.ToVector3(node.Scale);
        Matrix4x4 m1 = Matrix4x4.TRS(translation, Quaternion.identity, scale);
        Debug.Log("identity: " + Quaternion.identity.ToString());
        Debug.Log("rotation: " + rotation.ToString());
        Matrix4x4 m2 = ArrayConverter.ToMatrix4x4(node.Matrix);
        Matrix4x4 matrix = parentMatrix * m1 * m2;

        if (node.Meshes != null)
        {
            foreach (string meshID in node.Meshes)
                LoadMesh(m_model.Meshes[meshID], matrix);
        }
        if (node.Camera != null)
        {
            string cameraID = node.Camera;
            LoadCamera(m_model.Cameras[cameraID]);
        }
		if (node.Children != null)
		{
			foreach (string childID in node.Children)
				LoadNode(m_model.Nodes[childID], matrix);
		}
    }

    private void LoadCamera(glTFLoader.Schema.Camera camera)
    {
		GameObject obj = new GameObject();
		UnityEngine.Camera res = obj.AddComponent<UnityEngine.Camera>();
		Debug.Log("plop: " + res);
        res.name = camera.Name;
        if (camera.Type == glTFLoader.Schema.Camera.TypeEnum.perspective)
        {
            res.orthographic = false;
            res.fieldOfView = camera.Perspective.Yfov;
            res.farClipPlane = camera.Perspective.Zfar;
            res.nearClipPlane = camera.Perspective.Znear;
            if (camera.Perspective.AspectRatio != null)
                res.aspect = camera.Perspective.AspectRatio.Value;
        }
        else if (camera.Type == glTFLoader.Schema.Camera.TypeEnum.orthographic)
        {
            res.orthographic = true;
            res.farClipPlane = camera.Orthographic.Zfar;
            res.nearClipPlane = camera.Orthographic.Znear;
        }
        else
            throw new System.NotImplementedException("Unknown camera type");
    }

    private void LoadMesh(glTFLoader.Schema.Mesh mesh, Matrix4x4 matrix)
    {
        string name = mesh.Name;
        foreach (Primitive primitive in mesh.Primitives)
            LoadPrimitive(primitive, name, matrix);
    }

    private void LoadPrimitive(Primitive primitive, string name, Matrix4x4 matrix)
    {
        if (primitive.Mode != Primitive.ModeEnum.TRIANGLES)
            throw new System.NotImplementedException("Unsupported primitive mode");

        GameObject obj = new GameObject(name);
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        MeshFilter filter = obj.AddComponent<MeshFilter>();
        filter.mesh = new UnityEngine.Mesh();

        string posID = primitive.Attributes["POSITION"];
        Vector3[] positions = new AccessorReader(m_model, posID).ReadVector3Array();
        for (int i = 0; i < positions.Length; ++i)
            positions[i] = matrix * new Vector4(positions[i].x, positions[i].y, positions[i].z, 1);
        filter.mesh.vertices = positions;

        string indicesID = primitive.Indices;
        int[] indices = new AccessorReader(m_model, indicesID).ReadIntArray();
        filter.mesh.triangles = indices;

        if (primitive.Attributes.ContainsKey("TEXCOORD_0"))
        {
            string texCoordID = primitive.Attributes["TEXCOORD_0"];
            try
            {
                Vector2[] texCoord = new AccessorReader(m_model, texCoordID).ReadVector2Array();
                filter.mesh.uv = texCoord;
            }
            catch (System.TypeLoadException e) {; }
        }

        string materialID = primitive.Material;
        UnityEngine.Material material = LoadMaterial(m_model.Materials[materialID]);
        renderer.material = material;

        filter.mesh.RecalculateBounds();
        filter.mesh.RecalculateNormals();
    }

    private UnityEngine.Material LoadMaterial(glTFLoader.Schema.Material material)
    {
        string techniqueID = material.Technique;
		if (techniqueID != null) 
			LoadTechnique(m_model.Techniques[techniqueID]);

        UnityEngine.Material res = new UnityEngine.Material(UnityEngine.Shader.Find(" Diffuse"));
        res.name = material.Name;

		if (material.Values != null)
		{
	        if (material.Values.ContainsKey("ambient"))
	        {
	            JArray ambientArray = material.Values["ambient"] as JArray;
	            int[] ambient = ambientArray.ToObject<int[]>();
	            res.SetColor("_Color", ArrayConverter.ToColor(ambient));
	        }
	        if (material.Values.ContainsKey("emission"))
	        {
	            JArray emissionArray = material.Values["emission"] as JArray;
	            int[] emission = emissionArray.ToObject<int[]>();
	            res.SetColor("_EmissionColor", ArrayConverter.ToColor(emission));
	        }
	        if (material.Values.ContainsKey("diffuse"))
	        {
	            if (material.Values["diffuse"] is string)
	            {
	                string diffuseTextureID = material.Values["diffuse"] as string;
	                Texture2D texture = LoadTexture(m_model.Textures[diffuseTextureID]);
	                res.mainTexture = texture;
	            }
	        }
		}
        return res;
    }

    private static Dictionary<Sampler.WrapSEnum, TextureWrapMode> s_wrapSMode = new Dictionary<Sampler.WrapSEnum, TextureWrapMode>()
    {
        {Sampler.WrapSEnum.CLAMP_TO_EDGE,   TextureWrapMode.Clamp},
        {Sampler.WrapSEnum.REPEAT,          TextureWrapMode.Repeat},
        //{Sampler.WrapSEnum.MIRRORED_REPEAT, TextureWrapMode.Repeat},
    };

    private static Dictionary<Sampler.WrapTEnum, TextureWrapMode> s_wrapTMode = new Dictionary<Sampler.WrapTEnum, TextureWrapMode>()
    {
        {Sampler.WrapTEnum.CLAMP_TO_EDGE,   TextureWrapMode.Clamp},
        {Sampler.WrapTEnum.REPEAT,          TextureWrapMode.Repeat},
        //{Sampler.WrapTEnum.MIRRORED_REPEAT, TextureWrapMode.Repeat},
    };

    private Texture2D LoadTexture(glTFLoader.Schema.Texture texture)
    {
        string path = "Assets/cache_" + texture.Name + ".png";

        string sourceID = texture.Source;
        glTFLoader.Schema.Image image = m_model.Images[sourceID];
        Bitmap bitmap = image.Uri;
        bitmap.Save(path);

        byte[] data = File.ReadAllBytes(path);
        Texture2D res = new Texture2D(2, 2);
        res.LoadImage(data); // auto resize

        string samplerID = texture.Sampler;
        Sampler sampler = m_model.Samplers[samplerID];
        TextureWrapMode wrapS = s_wrapSMode[sampler.WrapS];
        TextureWrapMode wrapT = s_wrapTMode[sampler.WrapT];
        if (wrapS == wrapT)
            res.wrapMode = wrapS;
        else
            throw new System.NotImplementedException("Texture wrap mode not supported");
        return res;
    }

    private void LoadTechnique(Technique technique)
    {
        string programID = technique.Program;
        LoadProgram(m_model.Programs[programID]);
    }

    private void LoadProgram(Program program)
    {
        string vertexShaderID = program.VertexShader;
        string fragmentShaderID = program.FragmentShader;
        glTFLoader.Schema.Shader vertexShader = m_model.Shaders[vertexShaderID];
        glTFLoader.Schema.Shader fragmentShader = m_model.Shaders[fragmentShaderID];
        if (vertexShader.Type != glTFLoader.Schema.Shader.TypeEnum.VERTEX_SHADER ||
            fragmentShader.Type != glTFLoader.Schema.Shader.TypeEnum.FRAGMENT_SHADER)
            throw new System.Exception("Invalid shader type");
        string shaderString = MakeShaderString(vertexShader.Uri, fragmentShader.Uri);
        UnityEngine.Shader shader = CreateShader(shaderString);
        Debug.Log("shader: " + shaderString);
        Debug.Log("supported: " + shader.isSupported);
    }

    private UnityEngine.Shader CreateShader(string shaderString)
    {
        string resPath = Application.dataPath + "/resources";
        Directory.CreateDirectory(resPath);
        File.WriteAllText(resPath + "/workingshader.shader", shaderString);
        UnityEngine.Shader currentShader = Resources.Load("workingshader") as UnityEngine.Shader;
        string path = AssetDatabase.GetAssetPath(currentShader);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return currentShader;
    }

    private string MakeShaderString(string vertex, string fragment)
    {
        string shaderFormat = @"
        Shader ""Fast"" {{
            Properties {{
                _Color (""Color"", Color) = (1,1,1,1)
                _MainTex(""Albedo (RGB)"", 2D) = ""white"" {{}}
            }}
            SubShader {{
                Tags {{ ""Queue"" = ""Geometry"" }}

                Pass {{
                    GLSLPROGRAM

                    #ifdef VERTEX
                    {0}
                    #endif

                    #ifdef FRAGMENT
                    {1}
                    #endif

                    ENDGLSL
                }}
            }}
        }}";
        return string.Format(shaderFormat, vertex, fragment);
    }
}
