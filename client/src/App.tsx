import { Toaster } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import ErrorBoundary from "./components/ErrorBoundary";
import { ThemeProvider } from "./contexts/ThemeContext";
import OrderFlowApp from "./pages/OrderFlowApp";

/** Operations Ledger shell: dark navigation, warm workspace, cobalt action signals. */
export default function App() {
  return (
    <ErrorBoundary>
      <ThemeProvider defaultTheme="light">
        <TooltipProvider>
          <Toaster richColors position="top-right" />
          <OrderFlowApp />
        </TooltipProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}
