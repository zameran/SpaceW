using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ComponentPool : MonoBehaviour
{
	public int Count;
	
	[HideInInspector]
	public List<Component> Pool = new List<Component>();
	
	protected virtual void Update()
	{
		Count = Pool.Count;
	}
	
	protected virtual void OnDestroy()
	{
		for (var i = Pool.Count - 1; i >= 0; i--)
		{
			var element = Pool[i];
			
			if (element != null)
			{
				Object.DestroyImmediate(element.gameObject);
			}
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

public static class ComponentPool<T> where T : Component
{
	private static ComponentPool component;

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

			if (onAdd != null)
			{
				onAdd(element);
			}

			element.gameObject.hideFlags = HideFlags.DontSave;

			element.transform.parent = component.transform;

			element.gameObject.SetActive(false);

			component.Pool.Add(element);
		}

		return null;
	}

	public static T Pop(string name, Transform parent)
	{
		return Pop(name, parent, null);
	}

	public static T Pop(string name, Transform parent, System.Predicate<T> match)
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
						return QuickPop(name, parent, index);
					}
				}
				else
				{
					var element = QuickPop(name, parent, pool.Count - 1);

					if (element == null)
					{
						Debug.LogWarning("Popped a null element");

						Clean();

						if (pool.Count > 0)
						{
							return QuickPop(name, parent, pool.Count - 1);
						}
					}
					else
					{
						return element;
					}
				}
			}
		}

		return Helper.CreateGameObject(name, parent).AddComponent<T>();
	}

	public static void Clear()
	{
		Clear(t => Object.DestroyImmediate(t.gameObject));
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

	private static T QuickPop(string name, Transform parent, int index)
	{
		var pool = component.Pool;
		var element = pool[index];

		pool.RemoveAt(index);

		var tElement = (T)element;

		if (tElement != null)
		{
			tElement.transform.parent = parent;
			tElement.transform.localPosition = Vector3.zero;
			tElement.transform.localRotation = Quaternion.identity;
			tElement.transform.localScale = Vector3.one;

			tElement.gameObject.name = name;
			tElement.gameObject.hideFlags = HideFlags.None;
			tElement.gameObject.SetActive(true);
		}

		return tElement;
	}

	private static void UpdateComponent(bool allowCreation = true)
	{
		if (component == null)
		{
			var name = "ComponentPool<" + typeof(T).Name + ">";
			var root = GameObject.Find(name);

			if (root == null && allowCreation == true)
			{
				root = Helper.CreateGameObject(name);
			}

			if (root != null)
			{
				root.hideFlags = HideFlags.DontSave;

				Object.DontDestroyOnLoad(root);

				component = root.GetComponent<ComponentPool>();

				if (component == null && allowCreation == true)
				{
					Helper.BeginStealthSet(root);
					{
						component = root.AddComponent<ComponentPool>();
					}
					Helper.EndStealthSet();
				}
			}
		}
	}
}