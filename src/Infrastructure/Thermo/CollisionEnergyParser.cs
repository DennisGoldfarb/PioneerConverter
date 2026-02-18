namespace PioneerConverter.Infrastructure.Thermo;

public static class CollisionEnergyParser
{
    public static bool TryParseCollisionEnergyEv(string energyValue, out float ev)
    {
        ev = 0.0f;
        if (energyValue.Contains(','))
        {
            float sum = 0.0f;
            int count = 0;
            string[] energyValues = energyValue.Split(',');
            foreach (string value in energyValues)
            {
                if (float.TryParse(value.Trim(), out float parsedValue))
                {
                    sum += parsedValue;
                    count++;
                }
            }

            if (count == 0)
            {
                return false;
            }

            ev = sum / count;
            return true;
        }

        return float.TryParse(energyValue, out ev);
    }
}
