using UnityEngine;

[CreateAssetMenu(fileName = "CreaturesList", menuName = "GameContent/Creatures List")]
public class CreaturesList : ScriptableObject
{
    private static CreaturesList _instance;
    public static CreaturesList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<CreaturesList>("Lists/CreaturesList");
            }
            return _instance;
        }
    }

    [SerializeField] private Entity[] creatures = null;
    public Entity[] Creatures => creatures;
}
