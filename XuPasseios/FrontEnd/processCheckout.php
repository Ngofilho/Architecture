<?php
session_start();

$checkoutMessage = $_SESSION["checkoutMessage"];
    
$message = new stdClass();

$date = new DateTime('now', new DateTimeZone('UTC'));
$iso_format = $date->format("Y-m-d\TH:i:sP");

    $message->MessageId = generate_uuid();
    $message->MesssageVersion = "1.0";
    $message->Origin = "Checkout";
    $message->Destiny = "Order";
    $message->DateOfSending->$iso_format;
    $message->Customers = array (1);
    
    foreach ($message->Customers as $key => $value) {
        $message->Customers[$key] = array
        (
            "ClientId" => generate_uuid()
        );
    }
    
    echo print_r($checkoutMessage) . "Checkout Message <br/>";
    $message->OrderDetails = array(count($checkoutMessage));

    foreach ($checkoutMessage as $key  => $value)
    {
        $message->OrderDetails[$key] = array(
            'ItemId' => $value["productId"],
            'Quantity' => $value["quantity"],
            'price' => $value["price"],
            'ClientId' => generate_uuid()
        );
    }
    

    $payload = json_encode($message);

$ch = curl_init();
$item = 0;

$api_url = "https://localhost:7127/api/Checkout";

curl_setopt($ch, CURLOPT_CUSTOMREQUEST, "POST");
curl_setopt($ch, CURLOPT_POSTFIELDS, $payload);
curl_setopt($ch, CURLOPT_HTTPHEADER, array(
    'Content-type: application/json',
    'accept: */*',
    'Content-Length: ' . strlen($payload)
    )
);

curl_setopt($ch, CURLOPT_SSL_VERIFYPEER , false);
curl_setopt($ch, CURLOPT_SSL_VERIFYHOST , false);


curl_setopt($ch, CURLOPT_URL, $api_url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

$response = curl_exec($ch);

if (curl_errno($ch))
{
    echo 'cURL error: '. curl_error($ch);
    echo '<br/>';
    echo var_dump($response);
}

curl_close($ch);
/*
$data = json_decode($response, true);

if(!$data)
{
    echo '<br/>';
    echo 'Failed to decode JSON response.';
    echo '<br/>';
    var_dump($response);
}
*/
function generate_uuid(){
        // Generate 16 bytes (128 bits) of random data
        $data = random_bytes(16);

        // Set version to 0100 (UUID version 4)
        $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
    
        // Set bits 6-7 of the clock_seq_hi_and_reserved to 10
        $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
    
        // Format the bytes into a UUID string
        return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
?>
