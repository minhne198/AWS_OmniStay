---
name: Azure Horizon
colors:
  surface: '#f9f9f9'
  surface-dim: '#dadada'
  surface-bright: '#f9f9f9'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f3f3f3'
  surface-container: '#eeeeee'
  surface-container-high: '#e8e8e8'
  surface-container-highest: '#e2e2e2'
  on-surface: '#1a1c1c'
  on-surface-variant: '#414754'
  inverse-surface: '#2f3131'
  inverse-on-surface: '#f1f1f1'
  outline: '#727785'
  outline-variant: '#c1c6d6'
  surface-tint: '#005ac1'
  primary: '#0054b5'
  on-primary: '#ffffff'
  primary-container: '#006ce4'
  on-primary-container: '#f4f5ff'
  inverse-primary: '#adc6ff'
  secondary: '#7d5800'
  on-secondary: '#ffffff'
  secondary-container: '#ffb700'
  on-secondary-container: '#6b4b00'
  tertiary: '#3055a1'
  on-tertiary: '#ffffff'
  tertiary-container: '#4b6ebb'
  on-tertiary-container: '#f4f5ff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d8e2ff'
  primary-fixed-dim: '#adc6ff'
  on-primary-fixed: '#001a41'
  on-primary-fixed-variant: '#004494'
  secondary-fixed: '#ffdea9'
  secondary-fixed-dim: '#ffba26'
  on-secondary-fixed: '#271900'
  on-secondary-fixed-variant: '#5e4100'
  tertiary-fixed: '#d9e2ff'
  tertiary-fixed-dim: '#b0c6ff'
  on-tertiary-fixed: '#001945'
  on-tertiary-fixed-variant: '#1a438e'
  background: '#f9f9f9'
  on-background: '#1a1c1c'
  surface-variant: '#e2e2e2'
  booking-blue: '#003580'
  action-blue: '#006CE4'
  highlight-gold: '#FFB700'
  text-main: '#2A2A2E'
  success-green: '#008009'
typography:
  display-lg:
    fontFamily: Hanken Grotesk
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  headline-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 24px
  margin-desktop: 64px
  margin-mobile: 16px
  max-width: 1280px
---

## Brand & Style

The design system is engineered for high-intent utility within the travel and hospitality sector. It evokes a sense of reliability, transparency, and effortless planning. Taking inspiration from global booking platforms, the style is **Corporate / Modern**, prioritizing information density without sacrificing visual breathing room.

The aesthetic is characterized by:
- **Clarity:** A primary focus on legibility and clear calls to action.
- **Trust:** A systematic use of blue tones and structured layouts to reassure users during financial transactions.
- **Airiness:** Generous use of whitespace (negative space) to prevent "choice paralysis" and make complex search results feel manageable.

## Colors

The palette is anchored by a vibrant **Sky Blue** (Primary) used for interactive elements and primary buttons, ensuring high visibility. The **Deep Navy** (Tertiary) provides structural grounding and is used for headers and navigation to establish authority. 

**Highlight Gold** is reserved strictly for urgency or value-added signals, such as review scores, "deal" badges, and star ratings. The background strategy utilizes a very light neutral gray to subtly separate content sections from the pure white "paper" surfaces of the cards. Text is rendered in a near-black charcoal rather than pure black to reduce eye strain during long browsing sessions.

## Typography

This design system utilizes a dual-sans-serif approach to balance character with utility. **Hanken Grotesk** is used for headlines to provide a sharp, modern, and high-end feel. **Inter** is the workhorse for all UI elements, body copy, and data-rich tables, selected for its exceptional legibility at small sizes and its neutral, systematic tone.

Key typographic rules:
- **Price Display:** Prices should always use `headline-md` or larger and be bolded to ensure immediate recognition.
- **Hierarchical Contrast:** Use color (Neutral Gray vs. Text Main) rather than just size to distinguish between primary labels and secondary metadata.
- **Readability:** Maintain a maximum line length of 70 characters for body text descriptions.

## Layout & Spacing

The layout follows a **Fixed Grid** model for desktop to ensure search results remain consistent and scannable, centering the content within a 1280px container. On mobile and tablet, the system transitions to a fluid model with defined side margins.

- **Grid:** A 12-column system is used for the main search results and booking flows. Sidebars (filters) typically occupy 3 columns, while the main result feed occupies 9.
- **Rhythm:** An 8px linear scale (referenced as a 4px base unit) governs all padding and margins to maintain vertical rhythm.
- **Consistency:** Search bars should be anchored to the top of the viewport or hero section, spanning the full width of the grid container to emphasize the primary user task.

## Elevation & Depth

Hierarchy is established through **Tonal Layers** and subtle **Ambient Shadows**. 

- **Level 0 (Background):** Neutral light gray (#F5F5F5) acts as the canvas.
- **Level 1 (Cards/Content):** Pure white surfaces with a 1px border (#E0E0E0). No shadow is used here to keep the UI "airy."
- **Level 2 (Hover/Active):** When a hotel card is hovered, apply a soft, diffused shadow (0px 4px 12px rgba(0,0,0,0.08)) to indicate interactivity.
- **Level 3 (Modals/Popovers):** Used for date pickers and guest selectors. High-diffusion shadows (0px 12px 32px rgba(0,0,0,0.12)) are used to pull these elements far forward from the interface.

## Shapes

The design system uses a **Soft (0.25rem)** roundedness philosophy. This choice strikes a balance between the precision of a professional tool and the approachability of a travel service.

- **Buttons & Inputs:** Use the base 4px (0.25rem) radius.
- **Cards & Modals:** Use `rounded-lg` (8px / 0.5rem) to provide a gentle container for photography.
- **Status Indicators:** Pills and badges (like review scores) use a fully rounded "pill" shape to distinguish them from functional UI components.

## Components

### Buttons
- **Primary:** Solid Sky Blue with white text. High contrast, used for "Search" and "Book Now."
- **Secondary:** Outlined Sky Blue with a white background. Used for "Show Map" or "View Details."
- **Ghost:** No border or background, used for secondary navigation or "Cancel" actions.

### Search Bar
A signature component. Use a horizontal layout on desktop with distinct sections for "Location," "Dates," and "Guests," separated by vertical dividers. The entire unit should sit on a High-Contrast background or use a Level 2 shadow to pop against the hero image.

### Hotel Cards
Photography should occupy the left 30% of the card (on desktop). The right side contains the headline, rating badge (Gold), and price. Prices must be right-aligned or bottom-right-aligned to create a clear "scan line" for the user.

### Status Indicators
- **Review Scores:** A rounded square or pill with the Tertiary Blue background and white text.
- **Availability:** Use Success Green text for "Available" and Neutral Gray for "Sold Out."
- **Urgency Badges:** Use a "Highlight Gold" background for labels like "Great Value" or "Top Rated."

### Booking Forms
Input fields should use a 1px neutral border that turns Primary Blue on focus. Labels should always be visible (not just placeholder text) using `label-md` for maximum accessibility.