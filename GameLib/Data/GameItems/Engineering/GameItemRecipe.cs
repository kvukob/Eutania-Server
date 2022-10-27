using System.Collections;

namespace GameLib.Data.GameItems.Engineering;

public class GameItemRecipe : IEnumerable<RecipeItem>
{
    public IEnumerable<RecipeItem> RecipeItems { get; set; }

    public GameItemRecipe()
    {
        RecipeItems = new List<RecipeItem>();
    }

    public IEnumerator<RecipeItem> GetEnumerator()
    {
        return RecipeItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}