using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameObjectPool<T> : Pool<T> where T : Object
{
    public T Prefab { get; private set; }

    private readonly Transform _root;

    public GameObjectPool(T prefab) : this(prefab.name, () => Object.Instantiate(prefab))
    {
        Prefab = prefab;
    }

    public GameObjectPool(string name, Func<T> create) : base(create)
    {
        var rootGO = new GameObject(string.Format("'{0}' Pool", name));
        _root = rootGO.transform;
    }

    public override T Spawn()
    {
        var obj = base.Spawn();

        var gameObject = GetGameObject(obj);
        gameObject.SetActive(true);

        return obj;
    }

    public T Spawn(Vector3 position, Quaternion rotation, Transform parent)
    {
        var obj = Spawn();

        var gameObject = GetGameObject(obj);

        var transform = gameObject.transform;
        transform.position = position;
        transform.rotation = rotation;
        transform.parent = parent;

        return obj;
    }

    public override void Despawn(T target)
    {
        base.Despawn(target);

        CancelDespawn(target);

        var gameObject = GetGameObject(target);
        gameObject.SetActive(false);

        var transform = gameObject.transform;
        transform.SetParent(_root, false);
    }

    private static GameObject GetGameObject(T obj)
    {
        var component = obj as Component;
        if (component != null)
            return component.gameObject;

        return obj as GameObject;
    }
}