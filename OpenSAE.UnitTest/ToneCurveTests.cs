using OpenSAE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenSAE.UnitTest
{
    internal class ToneCurveTests
    {
        [Test]
        public void TestToneCurveMapping()
        {
            for (int i = 0; i < 256; i++)
            {
                var color = Color.FromArgb(255, (byte)i, (byte)i, (byte)i);

                var mapped = SymbolArtColorHelper.ApplyCurve(color);

                var reverseMapped = SymbolArtColorHelper.RemoveCurve(mapped);
                Assert.Multiple(() =>
                {
                    Assert.That(SymbolArtColorHelper.ApplyCurve(reverseMapped), Is.EqualTo(mapped));
                    Assert.That(SymbolArtColorHelper.RemoveCurve(mapped), Is.EqualTo(reverseMapped));
                });
            }

            // both black and white should be preserved when applying or removing tone curve
            var black = Color.FromArgb(255, 0, 0, 0);
            var white = Color.FromArgb(255, 255, 255, 255);
            
            Assert.Multiple(() =>
            {
                Assert.That(SymbolArtColorHelper.ApplyCurve(black), Is.EqualTo(black));
                Assert.That(SymbolArtColorHelper.ApplyCurve(white), Is.EqualTo(white));
                Assert.That(SymbolArtColorHelper.RemoveCurve(black), Is.EqualTo(black));
                Assert.That(SymbolArtColorHelper.RemoveCurve(white), Is.EqualTo(white));
            });
        }
    }
}
