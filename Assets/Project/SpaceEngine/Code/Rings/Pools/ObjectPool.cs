using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ObjectPool : MonoBehaviour
{
	public int Count;
	
	[HideInInspector]
	public List<Object> Pool = new List<Object>();
	
	protected virtual void Update()
	{
		Count = Pool.Count;
	}
	
	protected virtual void OnDestroy()
	{
		for (var i = Pool.Count - 1; i >= 0; i--)
		{
			Object.DestroyImmediate(Pool[i]);
		}
	}
	
#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		if (Application.isPlaying == false)
		{
			Helper.Destroy(gameObject);
		}
	}
#endif
}

public static class ObjectPool<T> where T : Object
{
	private static ObjectPool component;
	
	private static GameObject disabled;
	
	public static int Count
	{
		get
		{
			if (component != null)
			{
				return component.Pool.Count;
			}
			
			return 0;
		}
	}
	
	static ObjectPool()
	{
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			Debug.LogError("Attempting to use " + typeof(T).Name + " with SgtObjectPool. Use SgtComponentPool instead.");
		}
	}
	
	public static GameObject GameObject
	{
		get
		{
			UpdateComponent();
			
			if (disabled == null)
			{
				disabled = Helper.CreateGameObject("Disabled", component.transform);
				disabled.hideFlags = HideFlags.DontSave;
				
				disabled.SetActive(false);
			}
			
			return disabled;
		}
	}
	
	public static void Add(List<T> list, bool clearList = true)
	{
		Add(list, null, clearList);
	}
	
	public static void Add(List<T> list, System.Action<T> onAdd, bool clearList = true)
	{
		if (list != null)
		{
			for (var i = list.Count - 1; i >= 0; i--)
			{
				Add(list[i], onAdd);
			}
			
			if (clearList == true)
			{
				list.Clear();
			}
		}
	}
	
	public static T Add(T entry)
	{
		return Add(entry, null);
	}
	
	public static T Add(T element, System.Action<T> onAdd)
	{
		if (element != null)
		{
			UpdateComponent();
			
			element.hideFlags = HideFlags.DontSave;
			
			if (onAdd != null)
			{
				onAdd(element);
			}
			
			component.Pool.Add(element);
		}
		
		return null;
	}
	
	public static T Pop()
	{
		return Pop(null);
	}
	
	public static T Pop(System.Predicate<T> match)
	{
		UpdateComponent(false);
		
		if (component != null)
		{
			var pool = component.Pool;
			
			if (pool.Count > 0)
			{
				if (match != null)
				{
					var index = pool.FindIndex(o => match((T)o));
					
					if (index >= 0)
					{
						var element = QuickPop(index);
						
						if (element != null)
						{
							element.hideFlags = HideFlags.None;
						}
						
						return element;
					}
				}
				else
				{
					var element = QuickPop(pool.Count - 1);
					
					if (element == null)
					{
						Debug.LogWarning("Popped a null element");
						
						Clean();
						
						if (pool.Count > 0)
						{
							element = QuickPop(pool.Count - 1);
							
							element.hideFlags = HideFlags.None;
							
							return element;
						}
					}
					else
					{
						element.hideFlags = HideFlags.None;
						
						return element;
					}
				}
			}
		}
		
		return null;
	}
	
	public static void Clear()
	{
		Clear(t => Object.DestroyImmediate(t));
	}
	
	public static void Clear(System.Action<T> onClear)
	{
		UpdateComponent(false);
		
		if (onClear != null && component != null)
		{
			var pool = component.Pool;
			
			for (var i = 0; i < pool.Count; i++)
			{
				var element = pool[i];
				
				if (element != null)
				{
					onClear((T)element);
				}
			}
			
			pool.Clear();
		}
	}
	
	public static void ForEach(System.Action<T> onForEach)
	{
		UpdateComponent(false);
		
		if (onForEach != null && component != null)
		{
			var pool = component.Pool;
			
			for (var i = pool.Count - 1; i >= 0; i--)
			{
				var element = pool[i];
				
				if (element == null)
				{
					onForEach((T)element);
				}
				else
				{
					pool.RemoveAt(i);
				}
			}
		}
	}
	
	public static void Clean()
	{
		UpdateComponent(false);
		
		if (component != null)
		{
			var count = component.Pool.RemoveAll(e => e == null);
			
			if (count > 0)
			{
				Debug.LogWarning(typeof(T).Name + " pool contained " + count + " null elements");
			}
		}
	}
	
	private static T QuickPop(int index)
	{
		var pool    = component.Pool;
		var element = pool[index];
		
		pool.RemoveAt(index);
		
		return (T)element;
	}
	
	private static void UpdateComponent(bool allowCreation = true)
	{
		if (component == null)
		{
			var name = "ObjectPool<" + typeof(T).Name + ">";
			var root = GameObject.Find(name);
			
			if (root == null && allowCreation == true)
			{
				root = Helper.CreateGameObject(name);
			}
			
			if (root != null)
			{
				root.hideFlags = HideFlags.DontSave;
				
				//Object.DontDestroyOnLoad(root);
				
				component = root.GetComponent<ObjectPool>();
				
				if (component == null && allowCreation == true)
				{
					Helper.BeginStealthSet(root);
					{
						component = root.AddComponent<ObjectPool>();
					}
					Helper.EndStealthSet();
				}
			}
		}
	}
}