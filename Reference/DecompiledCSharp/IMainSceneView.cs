using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMainSceneView
{
	bool EnableCoinAvoidRects { get; set; }

	event Func<float> coinFlipComplete;

	void LoadBgImage(int bgId);

	void InitializeCoins();

	void InitializeRoles();

	void ClearRuntimeContent();

	void RemoveAllCoin();

	int GetActiveCoinCount();

	Transform GetRolePool();

	Transform GetCoinPool();

	Transform GetEffectPool(string effectPoolName);

	Transform GetTextPool();

	Transform GetHuanXingPool();

	Transform GetHuanxingPool();

	Transform GetCoinsSmokePool();

	Transform GetParticlePool();

	bool IsAddText();

	void ReduceTextCount();

	void AddTextCount();

	int GetCurrentTextCount();

	int GetMaxTextCount();

	void GetSceneBoundary(out Vector2 min, out Vector2 max);

	Vector2 GetRandomPositionInBoundary(float padding = 50f);

	Vector2 GetMostDenseCoinPosition(float gridSize = 100f);

	void SetCoinAvoidRects(IReadOnlyList<Rect> avoidRects);

	void AddGongfu();

	void AddDashe();
}
