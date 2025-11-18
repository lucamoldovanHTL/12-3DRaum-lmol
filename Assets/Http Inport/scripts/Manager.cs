using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic; // Wichtig für List<T>
using System.Text.RegularExpressions;

// Hilfsklasse zur Koppelung von URL und Daten-Container
// [System.Serializable] macht sie im Unity-Inspektor sichtbar
[System.Serializable]
public class TeacherConfig
{
    [Tooltip("Die URL zur Lehrer-Detailseite")]
    public string teacherURL;

    [Tooltip("Das ScriptableObject-Asset, in das die Daten gespeichert werden")]
    public TeacherData dataContainer;
}

public class TeacherImportManager : MonoBehaviour
{
    // -----------------------------------------------------------------
    // 1. Singleton-Implementierung
    // -----------------------------------------------------------------
    public static TeacherImportManager Instance { get; private set; }

    [Header("Lehrer Konfigurationen")]
    [Tooltip("Definieren Sie hier alle URLs und weisen Sie das jeweilige TeacherData-Asset zu.")]
    [SerializeField]
    private List<TeacherConfig> teacherConfigurations = new List<TeacherConfig>();

    // Die Klasse, die den Container für alle Details hält.
    private const string TargetContainerClass = "item first last even";

    private void Awake()
    {
        // Stellt sicher, dass nur eine Instanz des Managers existiert.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Da es nun mehrere Lehrer gibt, benennen wir den Manager generisch um.
        RenameGameObjectToGeneric();
    }

    void Start()
    {
        if (teacherConfigurations == null || teacherConfigurations.Count == 0)
        {
            Debug.LogError("Die Liste der Teacher-Konfigurationen ist leer!");
            return;
        }

        // **Iteration:** Starte den Importvorgang für JEDE Konfiguration in der Liste
        foreach (var config in teacherConfigurations)
        {
            if (config.dataContainer == null)
            {
                Debug.LogWarning($"Skipping URL {config.teacherURL}: TeacherData container is not assigned. Please assign an asset.");
                continue;
            }
            // Starte die Co-Routine mit den spezifischen Daten für diesen Lehrer
            StartCoroutine(FetchAndParseHTML(config.teacherURL, config.dataContainer));
        }
    }

    /// <summary>
    /// Führt die Webanfrage asynchron aus und parst das Ergebnis.
    /// Nimmt die URL und den spezifischen Daten-Container als Argumente entgegen.
    /// </summary>
    private IEnumerator FetchAndParseHTML(string url, TeacherData container)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("User-Agent", "UnityWebRequest/1.0");
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                // Schreibt den Fehler in den spezifischen Container
                container.UpdateData($"[FEHLER] {webRequest.error}", "N/A", "N/A", "N/A", "N/A");
            }
            else
            {
                string htmlContent = webRequest.downloadHandler.text;
                // Übergibt den spezifischen Container an die Parsing-Methode
                ParseAllDetailsFromHTML(htmlContent, container);
            }
        }
    }

    /// <summary>
    /// Parst alle Details mithilfe von Regex und speichert sie im ÜBERGEBENEN ScriptableObject.
    /// </summary>
    private void ParseAllDetailsFromHTML(string htmlContent, TeacherData container)
    {
        // 1. Extrahiere den gesamten Detail-Block (Regex-Logik bleibt gleich)
        string containerPattern = $@"(<div[^>]*class=[""'][^""']*?\b{TargetContainerClass}\b[^""']*?[""'][^>]*>[\s\S]*?</div>)";
        Match containerMatch = Regex.Match(htmlContent, containerPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (!containerMatch.Success)
        {
            container.UpdateData("Detail-Container nicht gefunden.", "N/A", "N/A", "N/A", "N/A");
            return;
        }

        string detailsBlock = containerMatch.Groups[1].Value;

        // ... (GetFieldValue und GetEmailValue Funktionen bleiben gleich,
        //     verwenden 'detailsBlock' zur Suche) ...

        // Die Funktionen GetFieldValue und GetEmailValue müssen in ParseAllDetailsFromHTML
        // definiert oder die Regex-Muster global definiert werden.

        // --- Hilfsfunktionen für Regex ---
        string GetFieldValue(string fieldClass)
        {
            string p = $@"(?:<div[^>]*class=[""']field\s*{fieldClass}[""'][^>]*>[\s\S]*?<span\s*class=[""']text[""'][^>]*>)(.*?)(?:</span>)";
            Match m = Regex.Match(detailsBlock, p, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : $"**{fieldClass} nicht gefunden**";
        }

        string GetEmailValue()
        {
            string p = $@"(?:<div[^>]*class=[""']field\s*Email[""'][^>]*>[\s\S]*?<a[^>]*href=[""']mailto:(.*?)[""'])";
            Match m = Regex.Match(detailsBlock, p, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : "**Email nicht gefunden**";
        }
        // --- Ende Hilfsfunktionen ---

        // 2. Werte extrahieren
        string name = GetFieldValue("Lehrername");
        string raum = GetFieldValue("Raum");
        string telefon = GetFieldValue("Telefon");
        string sprechstunde = GetFieldValue("SprStunde");
        string email = GetEmailValue();

        // 3. Daten speichern (im übergebenen Container)
        container.UpdateData(name, raum, telefon, sprechstunde, email);
    }

    /// <summary>
    /// Benennt das Game Object des Managers generisch um.
    /// </summary>
    private void RenameGameObjectToGeneric()
    {
        gameObject.name = "TeacherDataImporter_Manager";
    }
}