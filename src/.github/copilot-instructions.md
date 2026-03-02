# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- Use a consistent scroll pattern across pages: set `ScrollViewer.CanContentScroll="False"` on the `Page` and use a root `ScrollViewer` for vertically overflowable content.

## Error Handling
- When validation errors are returned as resource keys with pipe-delimited arguments, localize them using ILocalizationService before displaying to the user. This applies to error messages surfaced in the UI, such as EditingSlot.ValidationError in JornadaViewModel.