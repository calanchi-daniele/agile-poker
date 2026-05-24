import { BrowserRouter, Routes, Route } from "react-router-dom";
import { SplashPage } from "./pages/SplashPage";
import { PokerRoom } from "./pages/PokerRoom";

export default function App() {
  return (
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<SplashPage />} />
          <Route path="/room/:roomId" element={<PokerRoom />} />
        </Routes>
      </BrowserRouter>
  );
}