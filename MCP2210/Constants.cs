
// Copyright(c) 2016 Andrea Cervesato <sawk.ita@gmail.com>

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace MCP2210 {
    static class Constants {
        /// <summary>
        /// The vendor ID.
        /// </summary>
        public const int VendorId = 0x04d8;

        /// <summary>
        /// The product ID.
        /// </summary>
        public const int ProductId = 0x00de;

        /// <summary>
        /// The Hid packet size.
        /// </summary>
        public const int PacketsSize = 64;

        /// <summary>
        /// The maximum length of the password.
        /// </summary>
        public const int MaximumPasswordLength = 8;

        /// <summary>
        /// The number of general purpose lines.
        /// </summary>
        public const int NumberOfGeneralPurposeLines = 9;
    }
}
