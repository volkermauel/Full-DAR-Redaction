namespace RedactionLibrary
{
    public interface IRedactWordFile
    {
        byte[] JoinTwoFiles(byte[] first, byte[] second);
        byte[] JoinWithoutQuality(byte[] ar1, byte[] ar3, byte[] ar4, byte[] ar5, byte[] ar6, byte[] ar7);
        byte[] Redact(byte[] filenameToRead);
    }
}