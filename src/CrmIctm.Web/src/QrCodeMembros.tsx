import { PaginaQrCode } from "./QrCode";

export function PaginaQrCodeMembros({ igrejaId }: { igrejaId: string }) {
  return <PaginaQrCode igrejaId={igrejaId} paraMembros />;
}
