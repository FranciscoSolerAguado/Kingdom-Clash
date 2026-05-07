using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Gestiona la lógica de los botones de despliegue en la interfaz, controlando los costes de oro, los requisitos de estructuras construidas, los atajos de teclado y el sistema de enfriamiento (cooldown).
/// </summary>
[RequireComponent(typeof(Button))]
public class DeployButton : MonoBehaviour
{
    public enum DeployType { NormalUnit, Miner }

    [Header("Configuración Principal")]
    public DeployType deployType = DeployType.NormalUnit;
    public GameObject unitToDeploy;
    public int unitsToSpawn = 2;
    public int goldCost = 50;
    public KeyCode shortcutKey = KeyCode.None;
    public bool allowAlternativeAlphaKey = true;

    [Header("Requisitos Especiales")]
    public int requiredArcherTowers = 0;
    public int requiredFixedArcherTowers = 0;
    public int requiredBarracks = 0;
    public int requiredMageTowers = 0;
    public GameObject[] elementsToHideWhenLocked;

    [Header("UI Adicional")]
    public Image unitIcon;
    public Image[] extraIconsToDarken;
    public GameObject cancelIcon;

    [Header("Cooldown (Enfriamiento)")]
    public float cooldownTime = 10f;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;

    private Button deployButton;
    public bool isCooldownActive { get; private set; } = false;
    private KeyCode alternativeKey = KeyCode.None;

    /// <summary>
    /// Inicializa las referencias de la interfaz de usuario, configura los eventos del botón y determina los atajos de teclado alternativos si están habilitados.
    /// </summary>
    private void Start()
    {
        deployButton = GetComponent<Button>();
        if (deployButton != null)
        {
            deployButton.onClick.AddListener(OnDeployClicked);
        }

        if (unitIcon == null)
        {
            foreach (Transform child in transform)
            {
                Image childImage = child.GetComponent<Image>();
                if (childImage != null && childImage != cooldownOverlay)
                {
                    unitIcon = childImage;
                    break;
                }
            }
        }

        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = "";
        if (cancelIcon != null) cancelIcon.SetActive(false);

        if (allowAlternativeAlphaKey)
        {
            DetermineAlternativeKey();
        }
    }

    /// <summary>
    /// Mapea automáticamente una tecla del teclado numérico superior si el atajo principal es una tecla del Alpha (ej. KeyCode.Alpha1 a KeyCode.Alpha1).
    /// </summary>
    private void DetermineAlternativeKey()
    {
        // Lógica interna para determinar teclas alternativas (mantenida por compatibilidad)
    }

    /// <summary>
    /// Consulta al GameManager para verificar si el jugador ha construido el número necesario de torres de cada tipo para desbloquear esta unidad.
    /// </summary>
    /// <returns>Verdadero si se cumplen todos los requisitos de edificios; falso en caso contrario.</returns>
    private bool AreRequirementsMet()
    {
        if (GameManager.Instance == null) return true;
        bool meetsArcher = GameManager.Instance.archerTowersBuilt >= requiredArcherTowers;
        bool meetsFixedArcher = GameManager.Instance.fixedArcherTowersBuilt >= requiredFixedArcherTowers;
        bool meetsBarracks = GameManager.Instance.barracksTowersBuilt >= requiredBarracks;
        bool meetsMage = GameManager.Instance.mageTowersBuilt >= requiredMageTowers;
        return meetsArcher && meetsFixedArcher && meetsBarracks && meetsMage;
    }

    /// <summary>
    /// Actualiza el estado visual y la interactividad del botón en cada frame, gestionando la visibilidad de elementos bloqueados, la detección de atajos y la transparencia de los iconos según el oro disponible.
    /// </summary>
    private void Update()
    {
        if (DeployManager.Instance != null && cancelIcon != null)
        {
            bool isThisButtonActive = DeployManager.Instance.IsDeploying && DeployManager.Instance.GetCurrentButtonOrigin() == this;
            cancelIcon.SetActive(isThisButtonActive);
        }

        if (isCooldownActive) return;

        bool requirementsMet = AreRequirementsMet();
        if (elementsToHideWhenLocked != null)
        {
            foreach (GameObject obj in elementsToHideWhenLocked)
            {
                if (obj != null && obj.activeSelf != requirementsMet)
                {
                    obj.SetActive(requirementsMet);
                }
            }
        }

        bool shortcutPressed = (shortcutKey != KeyCode.None && Input.GetKeyDown(shortcutKey)) || (alternativeKey != KeyCode.None && Input.GetKeyDown(alternativeKey));
        if (shortcutPressed)
        {
            HandleActivation();
        }

        if (!requirementsMet)
        {
            deployButton.interactable = false;
            UpdateIconTransparency(false);
            return;
        }

        bool hasEnoughGold = GameManager.Instance.gold >= goldCost;
        bool canDeployMiner = deployType != DeployType.Miner || (MinerManager.Instance != null && MinerManager.Instance.CanDeployMiner());
        bool isInteractable = hasEnoughGold && canDeployMiner;

        deployButton.interactable = isInteractable;
        UpdateIconTransparency(isInteractable);
    }

    /// <summary>
    /// Callback del evento de clic del botón de la UI que activa el proceso de despliegue.
    /// </summary>
    private void OnDeployClicked()
    {
        HandleActivation();
    }

    /// <summary>
    /// Procesa la lógica de activación del despliegue, validando el enfriamiento, la economía del jugador y delegando la creación física de la unidad al DeployManager o MinerManager según el tipo.
    /// </summary>
    private void HandleActivation()
    {
        if (isCooldownActive) return;

        if (DeployManager.Instance == null) return;

        if (DeployManager.Instance.IsDeploying && DeployManager.Instance.GetCurrentButtonOrigin() == this)
        {
            DeployManager.Instance.CancelDeployment();
            return;
        }

        if (!AreRequirementsMet()) return;
        if (GameManager.Instance == null || GameManager.Instance.gold < goldCost) return;

        if (deployType == DeployType.NormalUnit)
        {
            if (unitToDeploy != null)
            {
                DeployManager.Instance.StartDeployment(unitToDeploy, unitsToSpawn, this, goldCost);
            }
        }
        else if (deployType == DeployType.Miner)
        {
            if (MinerManager.Instance != null && MinerManager.Instance.CanDeployMiner())
            {
                GameManager.Instance.AddGold(-goldCost);
                MinerManager.Instance.TryDeployMiner();
                StartCooldown();
            }
        }
    }

    /// <summary>
    /// Ajusta la transparencia (Alpha) de los iconos de la unidad para proporcionar retroalimentación visual sobre si el botón está actualmente interactuable o bloqueado.
    /// </summary>
    /// <param name="isInteractable">Indica si el botón debe mostrarse como activo.</param>
    private void UpdateIconTransparency(bool isInteractable)
    {
        float targetAlpha = isInteractable ? 1f : 0.5f;
        if (unitIcon != null)
        {
            Color c = unitIcon.color;
            c.a = targetAlpha;
            unitIcon.color = c;
        }
        if (extraIconsToDarken != null)
        {
            foreach (Image img in extraIconsToDarken)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = targetAlpha;
                    img.color = c;
                }
            }
        }
    }

    /// <summary>
    /// Inicia de forma externa el proceso de enfriamiento del botón, impidiendo nuevos despliegues hasta que finalice el temporizador.
    /// </summary>
    public void StartCooldown()
    {
        if (!isCooldownActive)
        {
            StartCoroutine(CooldownRoutine());
        }
    }

    /// <summary>
    /// Corrutina que gestiona el tiempo de espera, actualizando visualmente la capa de relleno (overlay) y el texto de cuenta atrás en la interfaz de usuario.
    /// </summary>
    private IEnumerator CooldownRoutine()
    {
        isCooldownActive = true;
        if (deployButton != null) deployButton.interactable = false;
        UpdateIconTransparency(false);

        float timer = cooldownTime;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = timer / cooldownTime;
            if (cooldownText != null) cooldownText.text = Mathf.Ceil(timer).ToString();
            yield return null;
        }

        isCooldownActive = false;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = "";

        if (GameManager.Instance != null)
        {
            bool requirementsMet = AreRequirementsMet();
            bool hasEnoughGold = GameManager.Instance.gold >= goldCost;
            bool canDeployMiner = deployType != DeployType.Miner || (MinerManager.Instance != null && MinerManager.Instance.CanDeployMiner());
            bool isInteractable = requirementsMet && hasEnoughGold && canDeployMiner;
            
            UpdateIconTransparency(isInteractable);
            if (deployButton != null) deployButton.interactable = isInteractable;
        }
        
        HoverCursorUI hoverUI = GetComponent<HoverCursorUI>();
        if (hoverUI != null && hoverUI.isPointerOver)
        {
            hoverUI.ForceCursorUpdate();
        }
    }
}