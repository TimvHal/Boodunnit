using UnityEngine;
public class AchievementHandler
{
    private static AchievementHandler _instance;

    public static AchievementHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                
                _instance = new AchievementHandler();
            }

            return _instance;
        }
    }
}