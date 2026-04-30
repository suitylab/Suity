using Suity.Helpers.Conversion;

namespace LZ4;

public static partial class LZ4Codec
{

	public static void Pickle(byte[] input, int inputOffset, int inputLength, ref byte[] output, ref int outputLength)
    {
        int maxLength = MaximumOutputLength(inputLength);
		if (output == null || output.Length < maxLength + 4)
        {
            output = new byte[maxLength + 4];
        }

        outputLength = Encode(input, inputOffset, inputLength, output, 4, output.Length - 4) + 4;
        EndianBitConverter.Little.CopyBytes(inputLength, output, 0);
    }

	public static byte[] Unpickle(byte[] input, int inputOffset, int inputLength)
    {
        int outputLength = EndianBitConverter.Little.ToInt32(input, inputOffset);
        byte[] output = new byte[outputLength];

        Decode(input, inputOffset + 4, inputLength - 4, output, 0, outputLength);

        return output;
    }
}
