using System;

public class ZBDUserProgress
{
    public long timePlayedInMinutes { get; set; }
    public string integrityToken { get; set; }
    public ZBDValidation validation { get; set; }
}

public class ZBDValidation
{
    public ZBDAppUsageStats appUsageStats { get; set; }
    public string userId { get; set; }
}

public class ZBDAppUsageStats
{
    public string packageName { get; set; }
    public long firstTimeStamp { get; set; }
    public long lastTimeStamp { get; set; }
    public long totalTimeInForeground { get; set; }
}

public class ZBDSignUp
{
    public string requestId { get; set; }
    public string userInput { get; set; }
    public string rewardsAppName { get; set; }
    public string integrityToken { get; set; }
    public ZBDValidation validation { get; set; }
}

public class ZBDRequestIdResponse
{
    public string requestId { get; set; }
    public string requestIdError { get; set; }
}

public class ZBDUserStatus
{
    public string status { get; set; }
    public string rewardsUserId { get; set; }
}

[Serializable]
public class ZBDSignUpResponse
{
    public bool success;
    public string message;
    public Data data;

    [Serializable]
    public class Data
    {
        public string id;
    }
}


[Serializable]
public class ZBDIPAddressResponse
{
    public bool success;
    public string message;
    public ZBDSupportedRegion data;
}

[Serializable]
public class ZBDSupportedRegion
{
    public string ipAddress;
    public bool isSupported;
    public string ipCountry;
    public string ipRegion;
}

[Serializable]
public class ZBDProgressResponse
{
    public bool success;
    public string message;
    public ZBDProgress data;
}

[Serializable]
public class ZBDProgress
{
    public double currentTotalPlaytime;
    public long currentEarnedToday;
    public double timeUntilNextReward;
    public long nextRewardAmount;
    public string nextRewardId;
}


public class ZBDWebViewMessage
{
    public string type;
    public string data;
}



public class ZBDWebViewSignUp
{
    public string gamertag;

}


public class ZBDWebViewResponse
{
    public int httpStatusCode;
    public Object response;

}

public class ZBDWebViewError
{
    public bool success;
    public string message;

}

public class ZBDFingerprintCallback
{
    public string requestId;
    public string error;

}
