// TeacherData.cs (Unverändert)
using UnityEngine;

[CreateAssetMenu(fileName = "TeacherData", menuName = "Teacher Data/Teacher Profile", order = 1)]
public class TeacherData : ScriptableObject
{
    [SerializeField] private string lehrerName = "Daten werden geladen...";
    [SerializeField] private string raum = "N/A";
    [SerializeField] private string telefon = "N/A";
    [SerializeField] private string sprechstunde = "N/A";
    [SerializeField] private string email = "N/A";

    public string LehrerName => lehrerName;
    public string Raum => raum;
    public string Telefon => telefon;
    public string Sprechstunde => sprechstunde;
    public string Email => email;

    public void UpdateData(string name, string room, string phone, string consultation, string mail)
    {
        lehrerName = name;
        raum = room;
        telefon = phone;
        sprechstunde = consultation;
        email = mail;

        Debug.Log($"[SO UPDATE] Daten für {name} erfolgreich gespeichert.");
    }
}