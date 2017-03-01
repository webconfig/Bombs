public class Packet
{
    public int Op;
    public byte[] datas;
    public Packet( int _command, byte[] _datas)
    {
        Op = _command;
        datas = _datas;
    }
}
