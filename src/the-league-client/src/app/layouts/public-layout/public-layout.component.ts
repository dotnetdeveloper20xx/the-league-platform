import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './public-layout.component.html'
})
export class PublicLayoutComponent {
  readonly currentYear = new Date().getFullYear();
}
