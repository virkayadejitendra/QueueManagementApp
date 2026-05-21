import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-customer-status-placeholder',
  templateUrl: './customer-status-placeholder.component.html',
  styleUrl: './customer-status-placeholder.component.scss'
})
export class CustomerStatusPlaceholderComponent {
  private readonly route = inject(ActivatedRoute);

  protected readonly queueEntryId = this.route.snapshot.paramMap.get('queueEntryId') ?? '';
  protected readonly trackingToken = this.route.snapshot.paramMap.get('trackingToken') ?? '';
}
