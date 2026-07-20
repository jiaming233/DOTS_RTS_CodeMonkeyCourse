using UnityEngine;

/// <summary>
/// 在原点创建一个立方体并添加刚体（Rigidbody）组件
/// 将此脚本挂载到场景中任意 GameObject 上即可生效
/// </summary>
public class SpawnCube : MonoBehaviour
{
    [Header("立方体设置")]
    [Tooltip("生成位置")]
    public Vector3 spawnPosition = Vector3.zero;

    [Tooltip("立方体大小")]
    public Vector3 cubeSize = Vector3.one;

    [Tooltip("立方体质量")]
    public float mass = 1f;

    [Tooltip("是否使用重力")]
    public bool useGravity = true;

    void Start()
    {
        Spawn();
    }

    /// <summary>
    /// 在原地创建一个立方体并添加 Rigidbody 组件
    /// </summary>
    public void Spawn()
    {
        // 创建立方体
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "SpawnedCube";
        cube.transform.position = spawnPosition;
        cube.transform.localScale = cubeSize;

        // 添加刚体组件
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = useGravity;

        Debug.Log($"立方体已创建！位置: {spawnPosition}, 大小: {cubeSize}, 质量: {mass}");
    }
}
