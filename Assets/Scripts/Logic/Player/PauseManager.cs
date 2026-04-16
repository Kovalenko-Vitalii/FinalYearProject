using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

// I need to do something with this script, it is bed that it has similar responsibilities as game orchestrator
public class PauseManager : MonoBehaviour
{
	public static PauseManager Instance { get; private set; }

	[SerializeField] private GameObject player;
	[SerializeField] private CharacterController characterController;
	[SerializeField] private MonoBehaviour playerMovement;
	[SerializeField] private CinemachineCamera cineCamera;
	[SerializeField] private CinemachineInputAxisController cineInput;
	public bool IsPaused { get; private set; } = false;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		SceneManager.sceneLoaded += OnSceneLoaded;

		StartCoroutine(RebindNextFrame());
	}

	private void OnDestroy()
	{
		if (Instance == this)
			SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		StartCoroutine(RebindNextFrame());
		IsPaused = false;
	}

	private IEnumerator RebindNextFrame()
	{
		yield return null;
		FindReferences();
	}

	private void FindReferences()
	{
		if (player == null) player = GameObject.FindGameObjectWithTag("Player");

		if (player != null)
		{
			if (characterController == null)
				characterController = player.GetComponent<CharacterController>();

			if (playerMovement == null)
				playerMovement = player.GetComponent<PlayerMovement>();
		}
		else
		{
			characterController = null;
			playerMovement = null;
		}

		if (cineCamera == null)
			cineCamera = Object.FindFirstObjectByType<CinemachineCamera>();

		if (cineInput == null)
			cineInput = Object.FindFirstObjectByType<CinemachineInputAxisController>();
	}

	private void EnsureRefs()
	{
		if (player == null || characterController == null || cineInput == null || cineCamera == null || playerMovement == null)
			FindReferences();
	}

	public void SetPlayerControl(bool enabled)
	{
		EnsureRefs();

		SoundManager.Instance.PausedSound(enabled);

		if (characterController != null)
			characterController.enabled = enabled;

		if (playerMovement != null)
			playerMovement.enabled = enabled;

		if (cineInput != null)
			cineInput.enabled = enabled;

		Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !enabled;

		IsPaused = !enabled;

		PlayerTickSystem.Instance.SetEnabled(enabled);
	}
}
