using Backend.Interface;
using Backend.Services;
using Moq;

namespace TestProject
{
    public class STLAnalyzerServiceTests
    {
        private readonly ISTLAnalyzerService _service;

        public STLAnalyzerServiceTests()
        {
            _service = new STLAnalyzerService(Mock.Of<IServiceProvider>());
        }

        private static MemoryStream BuildSingleTriangleBinaryStl()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true);

            writer.Write(new byte[80]); // header
            writer.Write(1); // triangle count

            // normal
            writer.Write(0f); writer.Write(0f); writer.Write(1f);
            // vertices (non-degenerate triangle away from the origin)
            writer.Write(0f); writer.Write(0f); writer.Write(10f);
            writer.Write(10f); writer.Write(0f); writer.Write(0f);
            writer.Write(0f); writer.Write(10f); writer.Write(0f);

            writer.Write((ushort)0); // attribute byte count
            writer.Flush();

            return new MemoryStream(ms.ToArray());
        }

        [Theory]
        [InlineData("PLA", 1.24)]
        [InlineData("PETG", 1.27)]
        [InlineData("ABS", 1.04)]
        [InlineData("Résine", 1.10)]
        [InlineData("Matériau inconnu", 1.24)] // repli sur PLA
        [InlineData(null, 1.24)] // repli sur PLA
        public async Task AnalyzeAsync_UsesDensityMatchingMateriau(string? materiau, decimal expectedDensity)
        {
            using var stream = BuildSingleTriangleBinaryStl();

            var metadata = await _service.AnalyzeAsync(stream, "test.stl", pieceId: 1, materiau);

            Assert.True(metadata.Volume > 0, "Le volume calculé doit être strictement positif pour ce triangle de test");
            var expectedWeight = Math.Round(metadata.Volume * expectedDensity, 2);
            Assert.Equal(expectedWeight, metadata.EstimatedWeight);
        }

        [Fact]
        public async Task AnalyzeAsync_DifferentMateriaux_ProduceDifferentWeightsForSameVolume()
        {
            using var streamPla = BuildSingleTriangleBinaryStl();
            using var streamPetg = BuildSingleTriangleBinaryStl();

            var metadataPla = await _service.AnalyzeAsync(streamPla, "test.stl", pieceId: 1, "PLA");
            var metadataPetg = await _service.AnalyzeAsync(streamPetg, "test.stl", pieceId: 1, "PETG");

            Assert.Equal(metadataPla.Volume, metadataPetg.Volume);
            Assert.NotEqual(metadataPla.EstimatedWeight, metadataPetg.EstimatedWeight);
        }
    }
}
