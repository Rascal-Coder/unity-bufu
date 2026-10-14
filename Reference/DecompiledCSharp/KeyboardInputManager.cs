using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputManager : SingletonMono<KeyboardInputManager>
{
	public delegate void KeyEvent(KeyCode keyCode);

	private List<KeyCode> _watchedKeys = new List<KeyCode>();

	private Dictionary<KeyCode, bool> _previousKeyStates = new Dictionary<KeyCode, bool>();

	public override bool DontDestory => true;

	public event KeyEvent OnKeyDown;

	public event KeyEvent OnKeyHold;

	public event KeyEvent OnKeyUp;

	protected override void OnAwake()
	{
		AddWatchedKey((KeyCode)119);
		AddWatchedKey((KeyCode)97);
		AddWatchedKey((KeyCode)115);
		AddWatchedKey((KeyCode)100);
		AddWatchedKey((KeyCode)112);
		AddWatchedKey((KeyCode)106);
		AddWatchedKey((KeyCode)107);
		AddWatchedKey((KeyCode)32);
		AddWatchedKey((KeyCode)27);
		AddWatchedKey((KeyCode)304);
		AddWatchedKey((KeyCode)303);
		AddWatchedKey((KeyCode)306);
		AddWatchedKey((KeyCode)305);
	}

	private void Update()
	{
		CheckKeyInputs();
	}

	private void CheckKeyInputs()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyCode watchedKey in _watchedKeys)
		{
			bool key = Input.GetKey(watchedKey);
			if (Input.GetKeyDown(watchedKey))
			{
				this.OnKeyDown?.Invoke(watchedKey);
			}
			if (key)
			{
				this.OnKeyHold?.Invoke(watchedKey);
			}
			if (Input.GetKeyUp(watchedKey))
			{
				this.OnKeyUp?.Invoke(watchedKey);
			}
			_previousKeyStates[watchedKey] = key;
		}
	}

	public void AddWatchedKey(KeyCode keyCode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!_watchedKeys.Contains(keyCode))
		{
			_watchedKeys.Add(keyCode);
			_previousKeyStates[keyCode] = false;
		}
	}

	public void RemoveWatchedKey(KeyCode keyCode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_watchedKeys.Remove(keyCode);
		_previousKeyStates.Remove(keyCode);
	}

	public bool IsKeyDown(KeyCode keyCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Input.GetKeyDown(keyCode);
	}

	public bool IsKeyHeld(KeyCode keyCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Input.GetKey(keyCode);
	}

	public bool IsKeyUp(KeyCode keyCode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Input.GetKeyUp(keyCode);
	}

	public float GetHorizontalInput()
	{
		return Input.GetAxis("Horizontal");
	}

	public float GetVerticalInput()
	{
		return Input.GetAxis("Vertical");
	}

	public Vector3 GetMousePosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Input.mousePosition;
	}

	public void ClearAllEvents()
	{
		this.OnKeyDown = null;
		this.OnKeyHold = null;
		this.OnKeyUp = null;
	}

	protected override void BeforOnDestroy()
	{
	}
}
