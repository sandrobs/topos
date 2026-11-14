export function LogoIgreja({ className = "" }: { className?: string }) {
  const classes = ["logo-igreja", className].filter(Boolean).join(" ");

  return (
    <span className={classes}>
      <img src="/logo-igreja.png" alt="Logo da Igreja de Cristo" />
    </span>
  );
}
