# Prompt 4 – Login Entry Page & Professional Backdrop

- Entry route: `/login` is the default unauthenticated page. Unauthenticated visitors are routed here.
- Split layout:
  - Left (~55%): Background image/canvas with brand lockup, value proposition, and trust bullets.
  - Right (~45%): Recruiter Velocity surface card with email/password fields, sign-in button, and demo quick-fill buttons.
- Fallback `<LoginBackdrop />`: Renders local asset when present or rich CSS/SVG gradient mesh fallback when absent.
- Quick fill buttons: `Use demo admin` and `Use demo customer` fill the emails without touching or hardcoding passwords.
