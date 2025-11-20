using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    private ConstructionComponent constructionComponent = null;

    private int levelIndex = 0;
    public int LevelIndex
    {
        get => levelIndex;
        set
        {
            if (value > levelIndex && value <= maxLevelIndex)
            {
                levelIndex = value;
            }
        }
    }

    private int maxLevelIndex = 0;

    public void Initialize()
    {
        constructionComponent = GetComponent<ConstructionComponent>();
        if (constructionComponent)
        {
            int count = constructionComponent.constructionLevelsData.Count;
            if (maxLevelIndex < count)
                maxLevelIndex = count;
        }
    }

    //public void SetLevel(int levelIndex)
    //{
    //    if (levelIndex > this.levelIndex && levelIndex <= maxLevelIndex)
    //    {
    //        this.levelIndex = levelIndex;
    //    }
    //}
}
