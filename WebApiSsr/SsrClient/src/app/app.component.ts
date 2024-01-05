import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'SsrClient';

  login = () => fetch("https://localhost:7056/api/login", { method: "post", credentials: "include" });
  testEndpoints = () => fetch("https://localhost:7056/api/test", { method: "get", credentials: "include" });
}
