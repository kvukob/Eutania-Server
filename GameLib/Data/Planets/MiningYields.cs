namespace GameLib.Data.Planets;

internal static class MiningYields
{ 
    public static double AetheaYield(string resource)
    {
        return resource switch
        {
            "Murissine" => 0.37,
            "Orythium" => 0.28,
            "Phytemicium" => 0.12,
            "Tausmium" => 0.24,
            _ => 0
        };
    }
    
    public static double AmautYield(string resource)
    {
        switch (resource)
        {
            case "Liavine": return 0.4;
            case "Murissine": return 0.37;
            case "Tausmium": return 0.24;
            default: return 0;
        }
    }
    
    public static double DraecarraYield(string resource)
    {
        switch (resource)
        {
            case "Murissine": return 0.9;
            case "Sangunalt": return 0.11;
            default: return 0;
        }
    }
    
    
    public static double EutaniaYield(string resource)
    {
        switch (resource)
        {
            case "Avanthium": return 0.28;
            case "Murissine": return 0.37;
            case "Phytemicium": return 0.12;
            case "Tausmium": return 0.24;
            default: return 0;
        }
    }
    
    public static double NecykeYield(string resource)
    {
        switch (resource)
        {
            case "Avanthium": return 0.29;
            case "Caelyrium": return 0.22;
            case "Liavine": return 0.5;
            default: return 0;
        }
    }
    
    public static double LuthienYield(string resource)
    {
        switch (resource)
        {
            case "Vohphos": return 1.01;
            default: return 0;
        }
    }
    
    public static double PsigenYield(string resource)
    {
        switch (resource)
        {
            case "Bulrium": return 1.01;
            default: return 0;
        }
    }
    
    public static double TsumaYield(string resource)
    {
        switch (resource)
        {
            case "Avanthium": return 0.41;
            case "Caelyrium": return 0.53;
            case "Mythine": return 0.07;
            default: return 0;
        }
    }
    
    
    
}