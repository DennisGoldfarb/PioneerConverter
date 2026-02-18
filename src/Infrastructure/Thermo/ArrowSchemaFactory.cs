using Apache.Arrow;
using Apache.Arrow.Types;

namespace PioneerConverter.Infrastructure.Thermo;

internal static class ArrowSchemaFactory
{
    public static Schema Create()
    {
        var massField = new Field.Builder()
            .Name("mz_array")
            .DataType(new ListType(FloatType.Default))
            .Nullable(false)
            .Build();
        var intensityField = new Field.Builder()
            .Name("intensity_array")
            .DataType(new ListType(FloatType.Default))
            .Nullable(false)
            .Build();
        var scanHeaderField = new Field.Builder()
            .Name("scanHeader")
            .DataType(StringType.Default)
            .Nullable(false)
            .Build();
        var scanNumberField = new Field.Builder()
            .Name("scanNumber")
            .DataType(Int32Type.Default)
            .Nullable(false)
            .Build();
        var basePeakMzField = new Field.Builder()
            .Name("basePeakMz")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var basePeakIntensityField = new Field.Builder()
            .Name("basePeakIntensity")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var packetTypeField = new Field.Builder()
            .Name("packetType")
            .DataType(Int32Type.Default)
            .Nullable(false)
            .Build();
        var retentionTimeField = new Field.Builder()
            .Name("retentionTime")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var lowMzField = new Field.Builder()
            .Name("lowMz")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var highMzField = new Field.Builder()
            .Name("highMz")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var ticField = new Field.Builder()
            .Name("TIC")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var centerMzField = new Field.Builder()
            .Name("centerMz")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var isolationWidthMzField = new Field.Builder()
            .Name("isolationWidthMz")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var collisionEnergyField = new Field.Builder()
            .Name("collisionEnergyField")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var collisionEnergyEvField = new Field.Builder()
            .Name("collisionEnergyEvField")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var msOrderField = new Field.Builder()
            .Name("msOrder")
            .DataType(UInt8Type.Default)
            .Nullable(false)
            .Build();

        return new Schema.Builder()
            .Field(massField)
            .Field(intensityField)
            .Field(scanHeaderField)
            .Field(scanNumberField)
            .Field(basePeakMzField)
            .Field(basePeakIntensityField)
            .Field(packetTypeField)
            .Field(retentionTimeField)
            .Field(lowMzField)
            .Field(highMzField)
            .Field(ticField)
            .Field(centerMzField)
            .Field(isolationWidthMzField)
            .Field(collisionEnergyField)
            .Field(collisionEnergyEvField)
            .Field(msOrderField)
            .Build();
    }
}
