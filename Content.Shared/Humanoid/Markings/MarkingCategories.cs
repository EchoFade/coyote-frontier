using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid.Markings
{
    [Serializable, NetSerializable]
    public enum MarkingCategories : byte
    {
        Special,
        Hair,
        FacialHair,
        Head,
        HeadTop,
        HeadSide,
        Snout,
        Chest,
        UndergarmentTop,
        UndergarmentBottom,
        Genital,
        Arms,
        Legs,
        Tail,
        Overlay,
        BaseChest,
        BaseHead,
        BaseLArm,
        BaseLFoot,
        BaseLHand,
        BaseLLeg,
        BaseRArm,
        BaseRFoot,
        BaseRHand,
        BaseRLeg,
    }

    public static class MarkingCategoriesConversion
    {
        /// <summary>
        /// Easy cheat cheet for converting BaseLayers to Bodyparts to hide!
        /// Basically, if I have a marking in THIS category selected, I want to hide the base layer of that body part.
        /// fucking kill me, dennis.
        /// </summary>
        public static bool Category2Layer(
            MarkingCategories category,
            out HumanoidVisualLayers baseLayerToHide
            )
        {
            baseLayerToHide = category switch
            {
                MarkingCategories.BaseChest => HumanoidVisualLayers.Chest,
                MarkingCategories.BaseHead => HumanoidVisualLayers.Head,
                MarkingCategories.BaseLArm => HumanoidVisualLayers.LArm,
                MarkingCategories.BaseLFoot => HumanoidVisualLayers.LFoot,
                MarkingCategories.BaseLHand => HumanoidVisualLayers.LHand,
                MarkingCategories.BaseLLeg => HumanoidVisualLayers.LLeg,
                MarkingCategories.BaseRArm => HumanoidVisualLayers.RArm,
                MarkingCategories.BaseRFoot => HumanoidVisualLayers.RFoot,
                MarkingCategories.BaseRHand => HumanoidVisualLayers.RHand,
                MarkingCategories.BaseRLeg => HumanoidVisualLayers.RLeg,
                _ => HumanoidVisualLayers.Disregard, // dont trigger the base layer hiding logic, dennis
            };
            return baseLayerToHide != HumanoidVisualLayers.Disregard;
        }

        public static MarkingCategories FromHumanoidVisualLayers(HumanoidVisualLayers layer)
        {
            return layer switch
            {
                HumanoidVisualLayers.Special => MarkingCategories.Special,
                HumanoidVisualLayers.Hair => MarkingCategories.Hair,
                HumanoidVisualLayers.FacialHair => MarkingCategories.FacialHair,
                HumanoidVisualLayers.Head => MarkingCategories.Head,
                HumanoidVisualLayers.HeadTop => MarkingCategories.HeadTop,
                HumanoidVisualLayers.HeadSide => MarkingCategories.HeadSide,
                HumanoidVisualLayers.Snout => MarkingCategories.Snout,
                HumanoidVisualLayers.Chest => MarkingCategories.Chest,
                HumanoidVisualLayers.UndergarmentTop => MarkingCategories.UndergarmentTop,
                HumanoidVisualLayers.UndergarmentBottom => MarkingCategories.UndergarmentBottom,
                HumanoidVisualLayers.Genital => MarkingCategories.Genital,
                HumanoidVisualLayers.RArm => MarkingCategories.Arms,
                HumanoidVisualLayers.LArm => MarkingCategories.Arms,
                HumanoidVisualLayers.RHand => MarkingCategories.Arms,
                HumanoidVisualLayers.LHand => MarkingCategories.Arms,
                HumanoidVisualLayers.LLeg => MarkingCategories.Legs,
                HumanoidVisualLayers.RLeg => MarkingCategories.Legs,
                HumanoidVisualLayers.LFoot => MarkingCategories.Legs,
                HumanoidVisualLayers.RFoot => MarkingCategories.Legs,
                HumanoidVisualLayers.Tail => MarkingCategories.Tail,
                HumanoidVisualLayers.RArmExtension => MarkingCategories.Arms, // Frontier: species-specific layer
                HumanoidVisualLayers.LArmExtension => MarkingCategories.Arms, // Frontier: species-specific layer
                _ => MarkingCategories.Overlay
            };
        }
    }
}
