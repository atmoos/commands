namespace progressTreeTest;

public interface IExportedProgressTest
{
    void ExportedProgressIsScaled();
    void ExportedProgressIsStrictlyMonotonic();
    void OutOfBoundExportedProgressIsIgnored();
}