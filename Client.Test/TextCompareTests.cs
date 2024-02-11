using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Test
{
    public class TextCompareTests
    {
        [Fact]
        public void TextCompare()
        {
            //Arange
            string newText = "Hallo";
            string oldTExt = "Halloo";
            var TextComparer = new TextCompare(oldTExt, newText );
            //Act
            var result = TextComparer.VergleicheObjekte();
            var Added = result.added;
            var removed = result.removed;   
            //Assert
            Added.Should().NotBeNull();
            removed.Should().NotBeNull();
            Added.Should().Be(newText);
            removed.Should().Be(oldTExt);

        }
    }
}
