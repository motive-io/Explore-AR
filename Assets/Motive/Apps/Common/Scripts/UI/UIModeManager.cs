// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Motive.Unity.AR;
using Motive.Unity.Utilities;

public class UIModeManager<T> : MonoBehaviour where T : UIModeManager<T>
{
	public Camera MainCamera;

	public Canvas ARCanvas;
	public Canvas MapCanvas;

	public GameObject[] ARModeObjects;
	public GameObject[] MapModeObjects;

	public virtual void SetARActive(bool active)
	{
		ObjectHelper.SetObjectsActive(ARModeObjects, active);

		if (ARCanvas)
		{
			ARCanvas.enabled = active;
		}

		if (active)
		{
			ARWorld.Instance.Activate();
		}
		else
		{
			ARWorld.Instance.Deactivate();
		}
	}

	public virtual void SetMapActive(bool active)
	{
        if (MapCanvas)
        {
            MapCanvas.enabled = active;
        }

        MainCamera.enabled = active;

		ObjectHelper.SetObjectsActive(MapModeObjects, active);
	}

	protected virtual void Start()
	{
	}

	static T sInstance = null;

	public static T Instance
	{
		get{return sInstance;}
	}

	protected virtual void Awake()
	{
		if (sInstance != null)
		{
			Debug.LogError("SingletonComponent.Awake: error " + name + " already initialized");
		}

		sInstance = (T)this;
	}

    public virtual void SetMapMode()
    {
        SetARActive(false);
        SetMapActive(true);
    }

    public virtual void SetARMode()
    {
        SetMapActive(false);
        SetMapActive(true);
    }
}
